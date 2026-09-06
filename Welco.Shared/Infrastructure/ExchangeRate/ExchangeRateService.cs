using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Options;
using Welco.Shared.Domain.Models;
using Welco.Shared.Persistance;
using ExchangeRateEntity = Welco.Shared.Domain.Models.ExchangeRate;
using SyncLogEntity = Welco.Shared.Domain.Models.ExchangeRateSyncLog;

namespace Welco.Shared.Infrastructure.ExchangeRate
{
    public class ExchangeRateService : IExchangeRateService
    {
        private readonly WelcoDbContext _db;
        private readonly IExchangeRateProvider _provider;
        private readonly ExchangeRateSettings _settings;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ExchangeRateService> _logger;

        public ExchangeRateService(
            WelcoDbContext db,
            IExchangeRateProvider provider,
            IOptions<ExchangeRateSettings> options,
            IMemoryCache cache,
            ILogger<ExchangeRateService> logger)
        {
            _db = db;
            _provider = provider;
            _settings = options.Value;
            _cache = cache;
            _logger = logger;
        }

        public async Task<ExchangeRateDto?> GetLatestRateAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken)
        {
            if (string.Equals(fromCurrency, toCurrency, StringComparison.OrdinalIgnoreCase))
            {
                var now = DateOnly.FromDateTime(DateTime.UtcNow.Date);
                return new ExchangeRateDto { BaseCurrency = fromCurrency.ToUpperInvariant(), TargetCurrency = toCurrency.ToUpperInvariant(), Rate = 1m, RateDate = now, Source = "identity", FetchedAt = DateTime.UtcNow };
            }

            var details = await ConvertWithDetailsAsync(1m, fromCurrency, toCurrency, cancellationToken);
            return new ExchangeRateDto
            {
                BaseCurrency = fromCurrency.ToUpperInvariant(),
                TargetCurrency = toCurrency.ToUpperInvariant(),
                Rate = details.Rate,
                RateDate = details.RateDate,
                Source = details.Source,
                FetchedAt = DateTime.UtcNow
            };
        }

        public async Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency, CancellationToken cancellationToken)
        {
            var res = await ConvertWithDetailsAsync(amount, fromCurrency, toCurrency, cancellationToken);
            return res.ConvertedAmount;
        }

        public async Task<ConversionResultDto> ConvertWithDetailsAsync(decimal amount, string fromCurrency, string toCurrency, CancellationToken cancellationToken)
        {
            if (amount < 0) throw new ArgumentException("Amount must be >= 0", nameof(amount));
            if (string.IsNullOrWhiteSpace(fromCurrency)) throw new ArgumentException("fromCurrency required");
            if (string.IsNullOrWhiteSpace(toCurrency)) throw new ArgumentException("toCurrency required");

            fromCurrency = fromCurrency.Trim().ToUpperInvariant();
            toCurrency = toCurrency.Trim().ToUpperInvariant();

            if (fromCurrency == toCurrency)
            {
                return new ConversionResultDto
                {
                    Amount = amount,
                    FromCurrency = fromCurrency,
                    ToCurrency = toCurrency,
                    Rate = 1m,
                    ConvertedAmount = Decimal.Round(amount, GetDecimalDigits(toCurrency)),
                    RateDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                    Source = "identity"
                };
            }

            var baseCurrency = _settings.BaseCurrency.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(baseCurrency)) baseCurrency = "USD";

            // Load latest rates for base currency (cached)
            var rates = await GetLatestRatesInternalAsync(baseCurrency, cancellationToken);

            var fromRate = fromCurrency == baseCurrency ? 1m : GetRateForCurrency(rates, fromCurrency);
            var toRate = toCurrency == baseCurrency ? 1m : GetRateForCurrency(rates, toCurrency);

            if (fromCurrency != baseCurrency && fromRate == null)
                throw new InvalidOperationException($"Missing exchange rate for {fromCurrency} (base {baseCurrency})");
            if (toCurrency != baseCurrency && toRate == null)
                throw new InvalidOperationException($"Missing exchange rate for {toCurrency} (base {baseCurrency})");

            decimal rate;
            if (fromCurrency == baseCurrency)
                rate = toRate!.Value;
            else if (toCurrency == baseCurrency)
                rate = 1m / fromRate!.Value;
            else
                rate = toRate!.Value / fromRate!.Value;

            // Financial precision: only round final converted amount per target currency
            var decimalDigits = GetDecimalDigits(toCurrency);
            var converted = Decimal.Round(amount * rate, decimalDigits, MidpointRounding.AwayFromZero);

            // Determine RateDate and Source from cached entry
            var rateDate = rates.Values.FirstOrDefault()?.RateDate ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);
            var source = rates.Values.FirstOrDefault()?.Source ?? _provider.ProviderName;

            return new ConversionResultDto
            {
                Amount = amount,
                FromCurrency = fromCurrency,
                ToCurrency = toCurrency,
                Rate = rate,
                ConvertedAmount = converted,
                RateDate = rateDate,
                Source = source
            };
        }

        public async Task<IReadOnlyCollection<ExchangeRateDto>> GetLatestRatesAsync(string baseCurrency, CancellationToken cancellationToken)
        {
            baseCurrency = NormalizeCode(baseCurrency);
            var dict = await GetLatestRatesInternalAsync(baseCurrency, cancellationToken);
            return dict.Values.Select(v => new ExchangeRateDto
            {
                Id = v.Id,
                BaseCurrency = v.BaseCurrency,
                TargetCurrency = v.TargetCurrency,
                Rate = v.Rate,
                RateDate = v.RateDate,
                Source = v.Source,
                FetchedAt = v.FetchedAt
            }).OrderBy(r => r.TargetCurrency).ToList();
        }

        public async Task<IReadOnlyCollection<ExchangeRateDto>> GetHistoricalRatesAsync(string baseCurrency, DateOnly date, CancellationToken cancellationToken)
        {
            baseCurrency = NormalizeCode(baseCurrency);
            var cacheKey = CacheKey(baseCurrency, date);
            if (_cache.TryGetValue(cacheKey, out Dictionary<string, CachedRate>? cached) && cached != null)
            {
                return cached.Values.Select(v => ToDto(v)).OrderBy(r => r.TargetCurrency).ToList();
            }

            var rates = await _db.ExchangeRates
                .AsNoTracking()
                .Include(r => r.BaseCurrency)
                .Include(r => r.TargetCurrency)
                .Where(r => r.BaseCurrency.Code == baseCurrency && r.RateDate == date && !r.IsDeleted)
                .ToListAsync(cancellationToken);

            if (rates.Count == 0)
            {
                // Fallback to latest if no historical
                return await GetLatestRatesAsync(baseCurrency, cancellationToken);
            }

            var dict = rates.ToDictionary(r => r.TargetCurrency.Code, r => new CachedRate
            {
                Id = r.Id,
                BaseCurrency = r.BaseCurrency.Code,
                TargetCurrency = r.TargetCurrency.Code,
                Rate = r.Rate,
                RateDate = r.RateDate,
                Source = r.Source,
                FetchedAt = r.FetchedAt
            }, StringComparer.OrdinalIgnoreCase);

            _cache.Set(cacheKey, dict, TimeSpan.FromMinutes(_settings.CacheExpirationMinutes > 0 ? _settings.CacheExpirationMinutes : 60));
            return dict.Values.Select(v => ToDto(v)).OrderBy(r => r.TargetCurrency).ToList();
        }

        public async Task<ExchangeRateSyncResult> SyncLatestRatesAsync(CancellationToken cancellationToken)
        {
            var baseCurrency = NormalizeCode(_settings.BaseCurrency);
            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
            return await SyncInternalAsync(baseCurrency, today, null, cancellationToken);
        }

        public async Task<ExchangeRateSyncResult> SyncHistoricalRatesAsync(DateOnly date, CancellationToken cancellationToken)
        {
            var baseCurrency = NormalizeCode(_settings.BaseCurrency);
            return await SyncInternalAsync(baseCurrency, date, date, cancellationToken);
        }

        private async Task<ExchangeRateSyncResult> SyncInternalAsync(string baseCurrency, DateOnly rateDate, DateOnly? providerDate, CancellationToken cancellationToken)
        {
            var started = DateTime.UtcNow;
            var log = new ExchangeRateSyncLog
            {
                Id = Guid.NewGuid(),
                StartedAt = started,
                BaseCurrency = baseCurrency,
                Source = _provider.ProviderName,
                Status = ExchangeRateSyncStatus.Success
            };
            log.MarkAsCreated("System");
            _db.ExchangeRateSyncLogs.Add(log);
            await _db.SaveChangesAsync(cancellationToken);

            try
            {
                ExchangeRateResponse response;
                if (providerDate.HasValue)
                {
                    var hist = await _provider.GetHistoricalRatesAsync(baseCurrency, providerDate.Value, cancellationToken);
                    if (hist == null)
                        throw new InvalidOperationException($"Provider returned no data for {baseCurrency} on {providerDate.Value:yyyy-MM-dd}");
                    response = hist;
                }
                else
                {
                    response = await _provider.GetLatestRatesAsync(baseCurrency, cancellationToken);
                }

                if (response.Rates == null || response.Rates.Count == 0)
                    throw new InvalidOperationException("Provider returned empty rates");

                // Validate base currency exists
                var baseCurr = await _db.Currencies.FirstOrDefaultAsync(c => c.Code == baseCurrency && !c.IsDeleted, cancellationToken);
                if (baseCurr == null)
                    throw new InvalidOperationException($"Base currency {baseCurrency} not found in Currencies table (seed required)");

                // Validate and filter rates
                var validRates = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                foreach (var kv in response.Rates)
                {
                    var code = kv.Key?.Trim().ToUpperInvariant();
                    if (string.IsNullOrWhiteSpace(code) || code.Length != 3) continue;
                    if (kv.Value <= 0) continue;
                    if (code == baseCurrency) continue;
                    validRates[code] = kv.Value;
                }

                if (validRates.Count == 0)
                    throw new InvalidOperationException("No valid rates after validation");

                // Bulk upsert for today's RateDate (do not overwrite history)
                var targetDate = response.Date;
                // Ensure we use rateDate param for consistency? Provider's date is authoritative (e.g., Frankfurter date)
                // Use provider date for storage
                var currencies = await _db.Currencies.Where(c => !c.IsDeleted).ToDictionaryAsync(c => c.Code, c => c, StringComparer.OrdinalIgnoreCase, cancellationToken);

                var count = 0;
                // NOTE: DbContext is configured with EnableRetryOnFailure, whose
                // SqlServerRetryingExecutionStrategy forbids user-initiated
                // transactions. All transactional work must run inside
                // CreateExecutionStrategy().ExecuteAsync so retries wrap the
                // whole transaction as a retriable unit.
                var strategy = _db.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
                    try
                    {
                        foreach (var kv in validRates)
                        {
                            if (!currencies.TryGetValue(kv.Key, out var targetCurr))
                            {
                                _logger.LogWarning("Skipping unknown currency {Code} not in DB", kv.Key);
                                continue;
                            }

                            var existing = await _db.ExchangeRates.FirstOrDefaultAsync(r =>
                                r.BaseCurrencyId == baseCurr.Id &&
                                r.TargetCurrencyId == targetCurr.Id &&
                                r.RateDate == targetDate, cancellationToken);

                            if (existing != null)
                            {
                                // Update today's rate if changed (idempotent)
                                if (existing.Rate != kv.Value || existing.Source != response.Source)
                                {
                                    existing.Rate = kv.Value;
                                    existing.Source = response.Source;
                                    existing.FetchedAt = response.FetchedAt;
                                    existing.MarkAsUpdated("System");
                                }
                            }
                            else
                            {
                                var er = new ExchangeRateEntity
                                {
                                    Id = Guid.NewGuid(),
                                    BaseCurrencyId = baseCurr.Id,
                                    TargetCurrencyId = targetCurr.Id,
                                    Rate = kv.Value,
                                    RateDate = targetDate,
                                    Source = response.Source,
                                    FetchedAt = response.FetchedAt
                                };
                                er.MarkAsCreated("System");
                                _db.ExchangeRates.Add(er);
                            }
                            count++;
                        }

                        await _db.SaveChangesAsync(cancellationToken);
                        await tx.CommitAsync(cancellationToken);
                    }
                    catch
                    {
                        await tx.RollbackAsync(cancellationToken);
                        throw;
                    }
                });

                // Invalidate cache
                var cacheKey = CacheKey(baseCurrency, targetDate);
                _cache.Remove(cacheKey);
                // Also cache latest (today)
                var latestKey = CacheKey(baseCurrency, targetDate);
                _cache.Remove(latestKey);

                log.CompletedAt = DateTime.UtcNow;
                log.RatesCount = count;
                log.Source = response.Source;
                log.Status = ExchangeRateSyncStatus.Success;
                log.MarkAsUpdated("System");
                await _db.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("ExchangeRate sync succeeded {Base} {Date} {Count} rates from {Source}", baseCurrency, targetDate, count, response.Source);

                return new ExchangeRateSyncResult
                {
                    Success = true,
                    BaseCurrency = baseCurrency,
                    Source = response.Source,
                    RatesCount = count,
                    RateDate = targetDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExchangeRate sync failed {Base} {Provider}", baseCurrency, _provider.ProviderName);
                log.CompletedAt = DateTime.UtcNow;
                log.Status = ExchangeRateSyncStatus.Failed;
                log.ErrorMessage = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;
                log.Source = _provider.ProviderName;
                log.MarkAsUpdated("System");
                await _db.SaveChangesAsync(cancellationToken);

                return new ExchangeRateSyncResult
                {
                    Success = false,
                    BaseCurrency = baseCurrency,
                    Source = _provider.ProviderName,
                    RatesCount = 0,
                    RateDate = rateDate,
                    ErrorMessage = ex.Message
                };
            }
        }

        private async Task<Dictionary<string, CachedRate>> GetLatestRatesInternalAsync(string baseCurrency, CancellationToken cancellationToken)
        {
            // Try cache first: key for today
            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
            var cacheKey = CacheKey(baseCurrency, today);
            if (_cache.TryGetValue(cacheKey, out Dictionary<string, CachedRate>? cached) && cached != null)
                return cached;

            // DB: get latest RateDate for base
            var baseCurr = await _db.Currencies.AsNoTracking().FirstOrDefaultAsync(c => c.Code == baseCurrency && !c.IsDeleted, cancellationToken);
            if (baseCurr == null)
                throw new InvalidOperationException($"Base currency {baseCurrency} not found");

            var latestDate = await _db.ExchangeRates
                .Where(r => r.BaseCurrencyId == baseCurr.Id && !r.IsDeleted)
                .OrderByDescending(r => r.RateDate)
                .Select(r => r.RateDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (latestDate == default)
            {
                _logger.LogWarning("No exchange rates found for {Base}, attempting provider sync", baseCurrency);
                var sync = await SyncLatestRatesAsync(cancellationToken);
                if (!sync.Success)
                    throw new InvalidOperationException($"No rates available for {baseCurrency} and sync failed: {sync.ErrorMessage}");

                latestDate = sync.RateDate;
            }

            var rates = await _db.ExchangeRates
                .AsNoTracking()
                .Include(r => r.BaseCurrency)
                .Include(r => r.TargetCurrency)
                .Where(r => r.BaseCurrencyId == baseCurr.Id && r.RateDate == latestDate && !r.IsDeleted)
                .ToListAsync(cancellationToken);

            var dict = rates.ToDictionary(r => r.TargetCurrency.Code, r => new CachedRate
            {
                Id = r.Id,
                BaseCurrency = r.BaseCurrency.Code,
                TargetCurrency = r.TargetCurrency.Code,
                Rate = r.Rate,
                RateDate = r.RateDate,
                Source = r.Source,
                FetchedAt = r.FetchedAt
            }, StringComparer.OrdinalIgnoreCase);

            // Also include base->self implied 1? Not stored.

            _cache.Set(cacheKey, dict, TimeSpan.FromMinutes(_settings.CacheExpirationMinutes > 0 ? _settings.CacheExpirationMinutes : 60));
            return dict;
        }

        private decimal? GetRateForCurrency(Dictionary<string, CachedRate> rates, string code)
        {
            if (rates.TryGetValue(code, out var cr)) return cr.Rate;
            return null;
        }

        private int GetDecimalDigits(string code)
        {
            // Try to get from DB cache? For simplicity use 2, but we can lookup currency's DecimalDigits
            // This is called sync; we do fast DB lookup fallback to 2
            var cur = _db.Currencies.AsNoTracking().FirstOrDefault(c => c.Code == code && !c.IsDeleted);
            return cur?.DecimalDigits ?? 2;
        }

        private static string NormalizeCode(string code) => string.IsNullOrWhiteSpace(code) ? "USD" : code.Trim().ToUpperInvariant();
        private static string CacheKey(string baseCurrency, DateOnly date) => $"exchange-rates:{baseCurrency}:{date:yyyy-MM-dd}";
        private static ExchangeRateDto ToDto(CachedRate r) => new() { Id = r.Id, BaseCurrency = r.BaseCurrency, TargetCurrency = r.TargetCurrency, Rate = r.Rate, RateDate = r.RateDate, Source = r.Source, FetchedAt = r.FetchedAt };

        private sealed class CachedRate
        {
            public Guid Id { get; set; }
            public string BaseCurrency { get; set; } = string.Empty;
            public string TargetCurrency { get; set; } = string.Empty;
            public decimal Rate { get; set; }
            public DateOnly RateDate { get; set; }
            public string Source { get; set; } = string.Empty;
            public DateTime FetchedAt { get; set; }
        }
    }
}
