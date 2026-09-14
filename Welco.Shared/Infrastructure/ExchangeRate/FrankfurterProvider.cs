using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Options;

namespace Welco.Shared.Infrastructure.ExchangeRate
{
    /// <summary>
    /// Real Frankfurter provider (ECB reference rates, https://api.frankfurter.app).
    /// Free, no key. ~30 fiat majors only — NO EGP/DZD/SAR/AED/QAR/PKR, so pairs
    /// outside ECB coverage 400 with "Missing exchange rate" unless an admin pins
    /// a manual rate. Select via ExchangeRateSettings:Provider = "Frankfurter".
    /// </summary>
    public class FrankfurterProvider : IExchangeRateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ExchangeRateSettings _settings;
        private readonly ILogger<FrankfurterProvider> _logger;

        public string ProviderName => "Frankfurter";

        public FrankfurterProvider(
            HttpClient httpClient,
            IOptions<ExchangeRateSettings> options,
            ILogger<FrankfurterProvider> logger)
        {
            _httpClient = httpClient;
            _settings = options.Value;
            _logger = logger;
            if (_httpClient.Timeout == System.Threading.Timeout.InfiniteTimeSpan)
                _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds > 0 ? _settings.TimeoutSeconds : 10);
        }

        public async Task<ExchangeRateResponse> GetLatestRatesAsync(string baseCurrency, IReadOnlyCollection<string>? targetCodes, CancellationToken cancellationToken)
        {
            var code = baseCurrency.Trim().ToUpperInvariant();
            var url = $"latest?base={Uri.EscapeDataString(code)}";
            _logger.LogInformation("Fetching rates from Frankfurter: {Url}", url);
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            return ToResponse(await response.Content.ReadFromJsonAsync<FrankfurterResponse>(cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException($"Frankfurter returned empty for {code}"), code);
        }

        public async Task<ExchangeRateResponse?> GetHistoricalRatesAsync(string baseCurrency, DateOnly date, IReadOnlyCollection<string>? targetCodes, CancellationToken cancellationToken)
        {
            var code = baseCurrency.Trim().ToUpperInvariant();
            var url = $"{date:yyyy-MM-dd}?base={Uri.EscapeDataString(code)}";
            _logger.LogInformation("Fetching historical rates from Frankfurter: {Url}", url);
            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadFromJsonAsync<FrankfurterResponse>(cancellationToken: cancellationToken);
            if (data == null || data.Rates == null || data.Rates.Count == 0) return null;
            return ToResponse(data, code);
        }

        private ExchangeRateResponse ToResponse(FrankfurterResponse data, string requestedBase)
        {
            if (data.Rates == null || data.Rates.Count == 0)
                throw new InvalidOperationException($"Frankfurter returned empty rates for {requestedBase}");
            var dict = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in data.Rates)
            {
                if (kv.Value <= 0) continue;
                var c = kv.Key.Trim().ToUpperInvariant();
                if (c.Length != 3) continue;
                dict[c] = kv.Value;
            }
            if (dict.Count == 0)
                throw new InvalidOperationException("No valid rates parsed from Frankfurter response");
            return new ExchangeRateResponse
            {
                BaseCurrency = requestedBase,
                Date = string.IsNullOrWhiteSpace(data.Date) ? DateOnly.FromDateTime(DateTime.UtcNow.Date) : DateOnly.Parse(data.Date),
                Rates = dict,
                Source = ProviderName,
                FetchedAt = DateTime.UtcNow
            };
        }

        private sealed class FrankfurterResponse
        {
            [JsonPropertyName("amount")] public decimal Amount { get; set; } = 1;
            [JsonPropertyName("base")] public string Base { get; set; } = string.Empty;
            [JsonPropertyName("date")] public string Date { get; set; } = string.Empty;
            [JsonPropertyName("rates")] public Dictionary<string, decimal> Rates { get; set; } = new();
        }
    }
}
