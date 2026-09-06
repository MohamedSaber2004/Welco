using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Options;

namespace Welco.Shared.Infrastructure.ExchangeRate
{
    // Primary provider per prompt.txt: https://cdn.jsdelivr.net/npm/@fawazahmed0/currency-api@latest/v1/currencies/usd.json
    // Frankfurter name kept for compatibility; actual BaseUrl is cdn.jsdelivr.net now
    public class FrankfurterExchangeRateProvider : IExchangeRateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ExchangeRateSettings _settings;
        private readonly ILogger<FrankfurterExchangeRateProvider> _logger;

        public string ProviderName => "FawazahmedCDN";

        public FrankfurterExchangeRateProvider(
            HttpClient httpClient,
            IOptions<ExchangeRateSettings> options,
            ILogger<FrankfurterExchangeRateProvider> logger)
        {
            _httpClient = httpClient;
            _settings = options.Value;
            _logger = logger;
            if (_httpClient.Timeout == System.Threading.Timeout.InfiniteTimeSpan)
                _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds > 0 ? _settings.TimeoutSeconds : 10);
        }

        public async Task<ExchangeRateResponse> GetLatestRatesAsync(string baseCurrency, CancellationToken cancellationToken)
        {
            var code = baseCurrency.Trim().ToLowerInvariant();
            var url = $"{code}.json";
            return await FetchCdnAsync(url, baseCurrency, null, cancellationToken);
        }

        public async Task<ExchangeRateResponse?> GetHistoricalRatesAsync(string baseCurrency, DateOnly date, CancellationToken cancellationToken)
        {
            var code = baseCurrency.Trim().ToLowerInvariant();
            var dateStr = date.ToString("yyyy-MM-dd");
            // CDN historical: https://cdn.jsdelivr.net/npm/@fawazahmed0/currency-api@2024-03-06/v1/currencies/usd.json
            // We try dated URL first; if 404, fallback to latest
            var url = $"{code}.json";
            try
            {
                // For historical we need to override BaseAddress to dated tag
                return await FetchCdnHistoricalAsync(code, dateStr, baseCurrency, cancellationToken);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning(ex, "Fawazahmed CDN: no rates for {Base} on {Date}", baseCurrency, dateStr);
                return null;
            }
        }

        private async Task<ExchangeRateResponse> FetchCdnAsync(string relativeUrl, string requestedBase, DateOnly? forcedDate, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching rates from Fawazahmed CDN: {Url} base {Base}", relativeUrl, requestedBase);
            var response = await _httpClient.GetAsync(relativeUrl, cancellationToken);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadFromJsonAsync<FawazahmedResponse>(cancellationToken: cancellationToken);
            if (data == null)
                throw new InvalidOperationException($"Fawazahmed CDN returned empty for {requestedBase}");

            // data is { date: "2024-03-06", usd: { eur: 0.92, egp: 50.94, ... } }
            var dictRaw = data.Rates;
            if (dictRaw == null || dictRaw.Count == 0)
                throw new InvalidOperationException($"Fawazahmed CDN returned empty rates for {requestedBase}");

            var date = forcedDate ?? (string.IsNullOrWhiteSpace(data.Date) ? DateOnly.FromDateTime(DateTime.UtcNow.Date) : DateOnly.Parse(data.Date));

            var dict = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in dictRaw)
            {
                if (kv.Value <= 0) continue;
                var code = kv.Key.Trim().ToUpperInvariant();
                if (code.Length != 3) continue;
                dict[code] = kv.Value;
            }

            if (dict.Count == 0)
                throw new InvalidOperationException("No valid rates parsed from Fawazahmed CDN response");

            return new ExchangeRateResponse
            {
                BaseCurrency = requestedBase.Trim().ToUpperInvariant(),
                Date = date,
                Rates = dict,
                Source = ProviderName,
                FetchedAt = DateTime.UtcNow
            };
        }

        private async Task<ExchangeRateResponse?> FetchCdnHistoricalAsync(string codeLower, string dateStr, string requestedBase, CancellationToken cancellationToken)
        {
            // Build absolute dated URL — ignore HttpClient BaseAddress for this call
            var datedUrl = $"https://cdn.jsdelivr.net/npm/@fawazahmed0/currency-api@{dateStr}/v1/currencies/{codeLower}.json";
            _logger.LogInformation("Fetching historical rates from Fawazahmed CDN: {Url}", datedUrl);
            using var req = new HttpRequestMessage(HttpMethod.Get, datedUrl);
            var response = await _httpClient.SendAsync(req, cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadFromJsonAsync<FawazahmedResponse>(cancellationToken: cancellationToken);
            if (data == null || data.Rates == null || data.Rates.Count == 0) return null;

            var date = DateOnly.Parse(data.Date ?? dateStr);
            var dict = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in data.Rates)
            {
                if (kv.Value <= 0) continue;
                var c = kv.Key.Trim().ToUpperInvariant();
                if (c.Length != 3) continue;
                dict[c] = kv.Value;
            }
            if (dict.Count == 0) return null;
            return new ExchangeRateResponse
            {
                BaseCurrency = requestedBase.Trim().ToUpperInvariant(),
                Date = date,
                Rates = dict,
                Source = ProviderName,
                FetchedAt = DateTime.UtcNow
            };
        }

        private sealed class FawazahmedResponse
        {
            [JsonPropertyName("date")] public string? Date { get; set; }
            // The rates object is keyed by base, e.g. "usd": { "eur": 0.92 }
            // We capture all remaining properties as rates via extension data
            [JsonExtensionData] public Dictionary<string, System.Text.Json.JsonElement>? ExtensionData { get; set; }

            [System.Text.Json.Serialization.JsonIgnore]
            public Dictionary<string, decimal>? Rates
            {
                get
                {
                    if (ExtensionData == null) return null;
                    // Find the first value that is an object containing rates (e.g. "usd")
                    foreach (var kv in ExtensionData)
                    {
                        if (kv.Value.ValueKind == System.Text.Json.JsonValueKind.Object)
                        {
                            try { return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, decimal>>(kv.Value.GetRawText()); }
                            catch { continue; }
                        }
                    }
                    return null;
                }
            }

            // Fallback direct properties for frankfurter compatibility
            [JsonPropertyName("base")] public string? Base { get; set; }
            [JsonPropertyName("rates")] public Dictionary<string, decimal>? FrankfurterRates { get; set; }
        }

        // Kept for backward compat parsing
        private sealed class FrankfurterResponse
        {
            [JsonPropertyName("amount")] public decimal Amount { get; set; } = 1;
            [JsonPropertyName("base")] public string Base { get; set; } = string.Empty;
            [JsonPropertyName("date")] public string Date { get; set; } = string.Empty;
            [JsonPropertyName("rates")] public Dictionary<string, decimal> Rates { get; set; } = new();
        }
    }

    public class ExchangeRateApiProvider : IExchangeRateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ExchangeRateSettings _settings;
        private readonly ILogger<ExchangeRateApiProvider> _logger;

        public string ProviderName => "ExchangeRateApi";

        public ExchangeRateApiProvider(HttpClient httpClient, IOptions<ExchangeRateSettings> options, ILogger<ExchangeRateApiProvider> logger)
        {
            _httpClient = httpClient;
            _settings = options.Value;
            _logger = logger;
            if (_httpClient.Timeout == System.Threading.Timeout.InfiniteTimeSpan)
                _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds > 0 ? _settings.TimeoutSeconds : 10);
        }

        public async Task<ExchangeRateResponse> GetLatestRatesAsync(string baseCurrency, CancellationToken cancellationToken)
        {
            var key = _settings.ApiKey;
            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException("ExchangeRateApi ApiKey is not configured");

            var url = $"latest/{Uri.EscapeDataString(baseCurrency.ToUpperInvariant())}";
            // ExchangeRate-API format: https://v6.exchangerate-api.com/v6/{key}/latest/USD
            _logger.LogInformation("Fetching rates from ExchangeRateApi: {Url}", url);
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadFromJsonAsync<ExchangeRateApiResponse>(cancellationToken: cancellationToken);
            if (data == null || data.Result != "success" || data.ConversionRates == null)
                throw new InvalidOperationException($"ExchangeRateApi failed: {data?.Result}");

            var date = DateOnly.FromDateTime(DateTime.UtcNow.Date);
            return new ExchangeRateResponse
            {
                BaseCurrency = baseCurrency.ToUpperInvariant(),
                Date = date,
                Rates = new Dictionary<string, decimal>(data.ConversionRates, StringComparer.OrdinalIgnoreCase),
                Source = ProviderName,
                FetchedAt = DateTime.UtcNow
            };
        }

        public async Task<ExchangeRateResponse?> GetHistoricalRatesAsync(string baseCurrency, DateOnly date, CancellationToken cancellationToken)
        {
            // ExchangeRate-API free tier does not support historical; return null to fallback to latest
            _logger.LogWarning("ExchangeRateApi does not support historical rates for {Base} {Date}, returning null", baseCurrency, date);
            await Task.CompletedTask;
            return null;
        }

        private sealed class ExchangeRateApiResponse
        {
            [JsonPropertyName("result")] public string Result { get; set; } = string.Empty;
            [JsonPropertyName("conversion_rates")] public Dictionary<string, decimal> ConversionRates { get; set; } = new();
        }
    }
}
