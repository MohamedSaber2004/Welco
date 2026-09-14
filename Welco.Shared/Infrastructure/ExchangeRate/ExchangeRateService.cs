using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Options;

namespace Welco.Shared.Infrastructure.ExchangeRate
{
    public class ExchangeRateService : IExchangeRateService
    {
        private readonly IExchangeRateProvider _provider;
        private readonly ExchangeRateSettings _settings;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ExchangeRateService> _logger;

        public ExchangeRateService(
            IExchangeRateProvider provider,
            IOptions<ExchangeRateSettings> options,
            IMemoryCache cache,
            ILogger<ExchangeRateService> logger)
        {
            _provider = provider;
            _settings = options.Value;
            _cache = cache;
            _logger = logger;
        }

        public async Task<ExchangeRateDto?> GetLatestRateAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken)
        {
            if (string.Equals(fromCurrency, toCurrency, StringComparison.OrdinalIgnoreCase))
            {
                var now = DateOnly.FromDateTime(DateTime.Now.Date);
                return new ExchangeRateDto { BaseCurrency = fromCurrency.ToUpperInvariant(), TargetCurrency = toCurrency.ToUpperInvariant(), Rate = 1m, RateDate = now, Source = "identity", FetchedAt = DateTime.Now };
            }

            var details = await ConvertWithDetailsAsync(1m, fromCurrency, toCurrency, cancellationToken);
            return new ExchangeRateDto
            {
                BaseCurrency = fromCurrency.ToUpperInvariant(),
                TargetCurrency = toCurrency.ToUpperInvariant(),
                Rate = details.Rate,
                RateDate = details.RateDate,
                Source = details.Source,
                FetchedAt = DateTime.Now
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
                    RateDate = DateOnly.FromDateTime(DateTime.Now.Date),
                    Source = "identity"
                };
            }

            var details = await _provider.ConvertAsync(fromCurrency, toCurrency, amount, cancellationToken);

            return new ConversionResultDto
            {
                Amount = details.Amount,
                FromCurrency = details.FromCurrency,
                ToCurrency = details.ToCurrency,
                Rate = details.Rate,
                ConvertedAmount = details.ConvertedAmount,
                RateDate = details.RateDate,
                Source = details.Source
            };
        }

        public async Task<CartTotalResultDto> ConvertCartTotalAsync(ConvertCartTotalRequest request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentException("Request required");
            if (string.IsNullOrWhiteSpace(request.ToCurrency)) throw new ArgumentException("ToCurrency required");
            if (request.Lines == null || request.Lines.Count == 0) throw new ArgumentException("Lines required");
            if (request.Lines.Count > 200) throw new ArgumentException("Too many lines (max 200)");

            var toCurrency = request.ToCurrency.Trim().ToUpperInvariant();
            var lines = new List<CartTotalLineResultDto>(request.Lines.Count);
            DateOnly rateDate = DateOnly.FromDateTime(DateTime.Now.Date);
            string source = _provider.ProviderName;

            foreach (var line in request.Lines)
            {
                if (line.Quantity <= 0) throw new ArgumentException($"Invalid quantity for '{line.Key}'");
                if (line.UnitAmount < 0) throw new ArgumentException($"Invalid amount for '{line.Key}'");
                var details = await _provider.ConvertAsync(line.FromCurrency, toCurrency, line.UnitAmount, cancellationToken);
                var nativeCeiled = CeilToDigits(line.UnitAmount, 0);
                var ceiledUnit = CeilToDigits(nativeCeiled * details.Rate, 0);
                var lineTotal = CeilToDigits(ceiledUnit * line.Quantity, 0);
                lines.Add(new CartTotalLineResultDto
                {
                    Key = line.Key ?? string.Empty,
                    FromCurrency = details.FromCurrency,
                    UnitAmount = line.UnitAmount,
                    CeiledUnitAmount = nativeCeiled,
                    Quantity = line.Quantity,
                    Rate = details.Rate,
                    ConvertedUnitAmount = ceiledUnit,
                    LineTotal = lineTotal
                });
                rateDate = details.RateDate;
                source = details.Source;
            }

            var subtotal = lines.Sum(l => l.LineTotal);
            return new CartTotalResultDto
            {
                ToCurrency = toCurrency,
                Lines = lines,
                Subtotal = subtotal,
                Total = subtotal,
                RateDate = rateDate,
                Source = source
            };
        }

        public async Task<IReadOnlyCollection<ExchangeRateDto>> GetLatestRatesAsync(string baseCurrency, CancellationToken cancellationToken)
        {
            baseCurrency = NormalizeCode(baseCurrency);
            var cacheKey = CacheKey(baseCurrency, DateOnly.FromDateTime(DateTime.Now.Date), _provider.ProviderName);
            if (_cache.TryGetValue(cacheKey, out Dictionary<string, ExchangeRateDto>? cached) && cached != null)
                return cached.Values.OrderBy(r => r.TargetCurrency).ToList();

            var targetCodes = new[] { "USD", "EUR", "GBP", "EGP", "SAR", "AED", "DZD" };
            var response = await _provider.GetLatestRatesAsync(baseCurrency, targetCodes, cancellationToken);
            var dict = response.Rates.ToDictionary(kv => kv.Key, kv => new ExchangeRateDto
            {
                BaseCurrency = response.BaseCurrency,
                TargetCurrency = kv.Key,
                Rate = kv.Value,
                RateDate = response.Date,
                Source = response.Source,
                FetchedAt = response.FetchedAt
            }, StringComparer.OrdinalIgnoreCase);

            _cache.Set(cacheKey, dict, TimeSpan.FromMinutes(_settings.CacheExpirationMinutes > 0 ? _settings.CacheExpirationMinutes : 60));
            return dict.Values.OrderBy(r => r.TargetCurrency).ToList();
        }

        public async Task<IReadOnlyCollection<ExchangeRateDto>> GetHistoricalRatesAsync(string baseCurrency, DateOnly date, CancellationToken cancellationToken)
        {
            baseCurrency = NormalizeCode(baseCurrency);
            var cacheKey = CacheKey(baseCurrency, date, _provider.ProviderName);
            if (_cache.TryGetValue(cacheKey, out Dictionary<string, ExchangeRateDto>? cached) && cached != null)
                return cached.Values.OrderBy(r => r.TargetCurrency).ToList();

            var targetCodes = new[] { "USD", "EUR", "GBP", "EGP", "SAR", "AED", "DZD" };
            var response = await _provider.GetHistoricalRatesAsync(baseCurrency, date, targetCodes, cancellationToken);
            if (response == null || response.Rates.Count == 0)
                return await GetLatestRatesAsync(baseCurrency, cancellationToken);

            var dict = response.Rates.ToDictionary(kv => kv.Key, kv => new ExchangeRateDto
            {
                BaseCurrency = response.BaseCurrency,
                TargetCurrency = kv.Key,
                Rate = kv.Value,
                RateDate = response.Date,
                Source = response.Source,
                FetchedAt = response.FetchedAt
            }, StringComparer.OrdinalIgnoreCase);

            _cache.Set(cacheKey, dict, TimeSpan.FromMinutes(_settings.CacheExpirationMinutes > 0 ? _settings.CacheExpirationMinutes : 60));
            return dict.Values.OrderBy(r => r.TargetCurrency).ToList();
        }

        public Task<ExchangeRateSyncResult> SyncLatestRatesAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("SyncLatestRatesAsync is deprecated; rates are fetched live from the provider");
            return Task.FromResult(new ExchangeRateSyncResult { Success = true, Source = _provider.ProviderName, RatesCount = 0, RateDate = DateOnly.FromDateTime(DateTime.Now.Date) });
        }

        public Task<ExchangeRateDto> SetManualRateAsync(SetManualRateRequest request, string updatedBy, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Manual rates are no longer supported; rates are fetched live from the provider");
        }

        public Task<bool> ClearManualRateAsync(string baseCurrency, string targetCurrency, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Manual rates are no longer supported; rates are fetched live from the provider");
        }

        public Task<ExchangeRateSyncResult> SyncHistoricalRatesAsync(DateOnly date, CancellationToken cancellationToken)
        {
            _logger.LogInformation("SyncHistoricalRatesAsync is deprecated; rates are fetched live from the provider");
            return Task.FromResult(new ExchangeRateSyncResult { Success = true, Source = _provider.ProviderName, RatesCount = 0, RateDate = date });
        }

        private static string NormalizeCode(string code) => string.IsNullOrWhiteSpace(code) ? "USD" : code.Trim().ToUpperInvariant();
        private static string CacheKey(string baseCurrency, DateOnly date, string? providerName = null)
        {
            var suffix = string.IsNullOrWhiteSpace(providerName) ? "" : $":{providerName.Trim().ToUpperInvariant()}";
            return $"exchange-rates:{baseCurrency}:{date:yyyy-MM-dd}{suffix}";
        }
        private static decimal CeilToDigits(decimal value, int digits)
        {
            var factor = 1m;
            for (var i = 0; i < digits; i++) factor *= 10m;
            return Math.Ceiling(value * factor) / factor;
        }
        private static int GetDecimalDigits(string code)
        {
            var common = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["USD"] = 2, ["EUR"] = 2, ["GBP"] = 2, ["EGP"] = 2,
                ["SAR"] = 2, ["AED"] = 2, ["DZD"] = 2, ["KWD"] = 3,
                ["BHD"] = 3, ["OMR"] = 3, ["JOD"] = 3, ["LYD"] = 3
            };
            return common.TryGetValue(code, out var d) ? d : 2;
        }
    }
}