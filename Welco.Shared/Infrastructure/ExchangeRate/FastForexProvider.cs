using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Options;

namespace Welco.Shared.Infrastructure.ExchangeRate
{
    public class FastForexProvider : IExchangeRateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ExchangeRateSettings _settings;
        private readonly ILogger<FastForexProvider> _logger;

        public string ProviderName => "FastForex";

        public FastForexProvider(
            HttpClient httpClient,
            IOptions<ExchangeRateSettings> options,
            ILogger<FastForexProvider> logger)
        {
            _httpClient = httpClient;
            _settings = options.Value;
            _logger = logger;
            if (_httpClient.Timeout == System.Threading.Timeout.InfiniteTimeSpan)
                _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds > 0 ? _settings.TimeoutSeconds : 10);
        }

        private string ApiKey
        {
            get
            {
                var key = _settings.ApiKey?.Trim();
                if (string.IsNullOrWhiteSpace(key))
                    throw new InvalidOperationException("FastForex ApiKey is not configured (ExchangeRateSettings:ApiKey)");
                return key;
            }
        }

        private static List<string> NormalizeTargets(IReadOnlyCollection<string>? targetCodes, string baseCode)
        {
            var codes = (targetCodes ?? [])
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c.Trim().ToUpperInvariant())
                .Where(c => c.Length == 3 && c != baseCode)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (codes.Count == 0)
                throw new InvalidOperationException("No target currencies to quote");
            return codes;
        }

        private async Task<(decimal Rate, DateOnly RateDate)> FetchOneAsync(string from, string to, CancellationToken ct)
        {
            from = from.Trim().ToUpperInvariant();
            to = to.Trim().ToUpperInvariant();
            var url = $"fetch-one?from={Uri.EscapeDataString(from)}&to={Uri.EscapeDataString(to)}&api_key={Uri.EscapeDataString(ApiKey)}";
            _logger.LogInformation("FastForex fetch-one {From}->{To}", from, to);
            var response = await _httpClient.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadFromJsonAsync<FastForexFetchOneResponse>(cancellationToken: ct)
                ?? throw new InvalidOperationException($"FastForex fetch-one returned empty for {from}->{to}");
            var dict = data.Result ?? data.Results;
            if (dict == null || !dict.TryGetValue(to, out var rate) || rate <= 0)
                throw new InvalidOperationException($"FastForex fetch-one missing target {to}");
            return (rate, ParseDailyDate(data.Updated, data.Date));
        }

        public async Task<ExchangeRateResponse> GetLatestRatesAsync(string baseCurrency, IReadOnlyCollection<string>? targetCodes, CancellationToken cancellationToken)
        {
            var code = baseCurrency.Trim().ToUpperInvariant();
            var targets = NormalizeTargets(targetCodes, code);
            _logger.LogInformation("Fetching daily rates from FastForex fetch-one for base {Base} ({Count} pairs)", code, targets.Count);

            var tasks = targets.Select(async t =>
            {
                var (rate, date) = await FetchOneAsync(code, t, cancellationToken);
                return (Target: t, Rate: rate, Date: date);
            });
            var results = await Task.WhenAll(tasks);

            var dict = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            var rateDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);
            foreach (var r in results)
            {
                dict[r.Target] = r.Rate;
                if (r.Date > rateDate) rateDate = r.Date;
            }
            if (results.Length > 0) rateDate = results.Max(r => r.Date);

            return new ExchangeRateResponse
            {
                BaseCurrency = code,
                Date = rateDate,
                Rates = dict,
                Source = ProviderName,
                FetchedAt = DateTime.UtcNow
            };
        }

        public async Task<ConversionResult> ConvertAsync(string fromCurrency, string toCurrency, decimal amount, CancellationToken cancellationToken)
        {
            if (amount < 0) throw new ArgumentException("Amount must be >= 0", nameof(amount));
            var from = fromCurrency.Trim().ToUpperInvariant();
            var to = toCurrency.Trim().ToUpperInvariant();
            if (from == to)
            {
                return new ConversionResult
                {
                    Amount = amount,
                    FromCurrency = from,
                    ToCurrency = to,
                    Rate = 1m,
                    ConvertedAmount = amount,
                    RateDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                    Source = ProviderName,
                    DecimalDigits = 2
                };
            }

            var (rate, rateDate) = await FetchOneAsync(from, to, cancellationToken);
            return new ConversionResult
            {
                Amount = amount,
                FromCurrency = from,
                ToCurrency = to,
                Rate = rate,
                ConvertedAmount = amount * rate,
                RateDate = rateDate,
                Source = ProviderName,
                DecimalDigits = 2
            };
        }

        private static DateOnly ParseDailyDate(string? updated, string? date)
        {
            if (!string.IsNullOrWhiteSpace(updated) && DateTime.TryParse(updated, out var dt))
                return DateOnly.FromDateTime(dt.ToUniversalTime().Date);
            if (!string.IsNullOrWhiteSpace(date) && DateOnly.TryParse(date, out var dd))
                return dd;
            return DateOnly.FromDateTime(DateTime.UtcNow.Date);
        }

        /// <summary>
        /// fetch-one shape: {"base":"USD","result":{"EGP":51.8158},"updated":"2026-09-15T14:17:21Z","ms":4}
        /// "results" plural accepted for tolerance.
        /// </summary>
        private sealed class FastForexFetchOneResponse
        {
            [JsonPropertyName("base")] public string? Base { get; set; }
            [JsonPropertyName("result")] public Dictionary<string, decimal>? Result { get; set; }
            [JsonPropertyName("results")] public Dictionary<string, decimal>? Results { get; set; }
            [JsonPropertyName("updated")] public string? Updated { get; set; }
            [JsonPropertyName("date")] public string? Date { get; set; }
            [JsonPropertyName("ms")] public int Ms { get; set; }
        }
    }
}
