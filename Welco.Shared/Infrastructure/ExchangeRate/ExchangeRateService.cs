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
        private readonly ILogger<ExchangeRateService> _logger;

        public ExchangeRateService(
            IExchangeRateProvider provider,
            IOptions<ExchangeRateSettings> options,
            ILogger<ExchangeRateService> logger)
        {
            _provider = provider;
            _settings = options.Value;
            _logger = logger;
        }

        /// <summary>Safety margin applied to conversion totals (never to display rates or identity conversions).</summary>
        private decimal ApplySafetyMargin(decimal marketRate)
        {
            var margin = _settings.SafetyMarginPercent;
            if (margin <= 0 || marketRate <= 0) return marketRate;
            return marketRate * (1m + margin / 100m);
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
                    ConvertedAmount = amount,
                    RateDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                    Source = "identity"
                };
            }

            var details = await _provider.ConvertAsync(fromCurrency, toCurrency, amount, cancellationToken);
            var safeRate = ApplySafetyMargin(details.Rate);

            return new ConversionResultDto
            {
                Amount = details.Amount,
                FromCurrency = details.FromCurrency,
                ToCurrency = details.ToCurrency,
                Rate = safeRate,
                ConvertedAmount = amount * safeRate,
                RateDate = details.RateDate,
                Source = details.Source,
                SafetyMarginPercent = _settings.SafetyMarginPercent > 0 ? _settings.SafetyMarginPercent : 0m
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
            DateOnly rateDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);
            string source = _provider.ProviderName;

            foreach (var line in request.Lines)
            {
                if (line.Quantity <= 0) throw new ArgumentException($"Invalid quantity for '{line.Key}'");
                if (line.UnitAmount < 0) throw new ArgumentException($"Invalid amount for '{line.Key}'");
                var details = await _provider.ConvertAsync(line.FromCurrency, toCurrency, line.UnitAmount, cancellationToken);
                var unitAmount = line.UnitAmount;
                var isIdentity = string.Equals(details.FromCurrency, toCurrency, StringComparison.OrdinalIgnoreCase);
                var safeRate = isIdentity ? details.Rate : ApplySafetyMargin(details.Rate);
                var convertedUnit = unitAmount * safeRate;
                var lineTotal = convertedUnit * line.Quantity;
                lines.Add(new CartTotalLineResultDto
                {
                    Key = line.Key ?? string.Empty,
                    FromCurrency = details.FromCurrency,
                    UnitAmount = unitAmount,
                    CeiledUnitAmount = unitAmount,
                    Quantity = line.Quantity,
                    Rate = safeRate,
                    ConvertedUnitAmount = convertedUnit,
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
                Source = source,
                SafetyMarginPercent = _settings.SafetyMarginPercent > 0 ? _settings.SafetyMarginPercent : 0m
            };
        }

        public async Task<IReadOnlyCollection<ExchangeRateDto>> GetLatestRatesAsync(string baseCurrency, CancellationToken cancellationToken)
        {
            baseCurrency = NormalizeCode(baseCurrency);
            var targetCodes = new[] { "USD", "EUR", "GBP", "EGP", "SAR", "AED", "DZD" };
            var response = await _provider.GetLatestRatesAsync(baseCurrency, targetCodes, cancellationToken);
            return response.Rates
                .Select(kv => new ExchangeRateDto
                {
                    BaseCurrency = response.BaseCurrency,
                    TargetCurrency = kv.Key,
                    Rate = kv.Value,
                    RateDate = response.Date,
                    Source = response.Source,
                    FetchedAt = response.FetchedAt
                })
                .OrderBy(r => r.TargetCurrency)
                .ToList();
        }

        private static string NormalizeCode(string code) => string.IsNullOrWhiteSpace(code) ? "USD" : code.Trim().ToUpperInvariant();
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