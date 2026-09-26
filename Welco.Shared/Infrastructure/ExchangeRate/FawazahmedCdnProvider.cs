using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Options;

namespace Welco.Shared.Infrastructure.ExchangeRate
{
    /// <summary>
    /// Live daily rates from the fawazahmed0/currency-api CDN static JSON
    /// (GET {base}.json, e.g. usd.json -&gt; {"date":"2026-09-25","usd":{"egp":48.1,...}}).
    /// No API key. Real data only: any download or parse failure throws, no fallback rates.
    /// </summary>
    public class FawazahmedCdnProvider : IExchangeRateProvider
    {
        public const string DefaultBaseUrl = "https://cdn.jsdelivr.net/npm/@fawazahmed0/currency-api@latest/v1/currencies";

        private readonly HttpClient _httpClient;
        private readonly ExchangeRateSettings _settings;
        private readonly ILogger<FawazahmedCdnProvider> _logger;

        public string ProviderName => "FawazahmedCDN";

        public FawazahmedCdnProvider(
            HttpClient httpClient,
            IOptions<ExchangeRateSettings> options,
            ILogger<FawazahmedCdnProvider> logger)
        {
            _httpClient = httpClient;
            _settings = options.Value;
            _logger = logger;
            if (_httpClient.Timeout == System.Threading.Timeout.InfiniteTimeSpan)
                _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds > 0 ? _settings.TimeoutSeconds : 10);
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

        private async Task<(Dictionary<string, decimal> Rates, DateOnly RateDate)> FetchTableAsync(string baseCode, CancellationToken ct)
        {
            var code = baseCode.Trim().ToUpperInvariant();
            var url = $"{code.ToLowerInvariant()}.json";
            _logger.LogInformation("Fawazahmed CDN fetch {Url} for base {Base}", url, code);
            var response = await _httpClient.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();
            using var doc = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: ct)
                ?? throw new InvalidOperationException($"Fawazahmed CDN returned empty for {code}");
            return ParseTable(doc, code);
        }

        private static (Dictionary<string, decimal> Rates, DateOnly RateDate) ParseTable(JsonDocument doc, string baseCode)
        {
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
                throw new InvalidOperationException($"Fawazahmed CDN returned invalid payload for {baseCode}");

            var rateDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);
            if (root.TryGetProperty("date", out var dateProp)
                && dateProp.ValueKind == JsonValueKind.String
                && DateOnly.TryParse(dateProp.GetString(), out var parsed))
            {
                rateDate = parsed;
            }

            JsonElement ratesElement = default;
            var found = false;
            foreach (var prop in root.EnumerateObject())
            {
                if (prop.Value.ValueKind == JsonValueKind.Object)
                {
                    ratesElement = prop.Value;
                    found = true;
                    break;
                }
            }
            if (!found)
                throw new InvalidOperationException($"Fawazahmed CDN returned empty rates for {baseCode}");

            var dict = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in ratesElement.EnumerateObject())
            {
                var target = kv.Name.Trim().ToUpperInvariant();
                if (target.Length != 3)
                    continue;
                if (kv.Value.ValueKind != JsonValueKind.Number || !kv.Value.TryGetDecimal(out var rate) || rate <= 0)
                    continue;
                dict[target] = rate;
            }
            if (dict.Count == 0)
                throw new InvalidOperationException($"Fawazahmed CDN returned no valid rates for {baseCode}");
            return (dict, rateDate);
        }

        public async Task<ExchangeRateResponse> GetLatestRatesAsync(string baseCurrency, IReadOnlyCollection<string>? targetCodes, CancellationToken cancellationToken)
        {
            var code = baseCurrency.Trim().ToUpperInvariant();
            var targets = NormalizeTargets(targetCodes, code);
            _logger.LogInformation("Fetching daily rates from Fawazahmed CDN for base {Base} ({Count} targets)", code, targets.Count);

            var (table, rateDate) = await FetchTableAsync(code, cancellationToken);
            var dict = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            foreach (var t in targets)
            {
                if (!table.TryGetValue(t, out var rate))
                    throw new InvalidOperationException($"Fawazahmed CDN missing target {t} for base {code}");
                dict[t] = rate;
            }

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

            var (table, rateDate) = await FetchTableAsync(from, cancellationToken);
            if (!table.TryGetValue(to, out var rate))
                throw new InvalidOperationException($"Fawazahmed CDN missing target {to} for base {from}");
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
    }
}
