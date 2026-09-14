using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Options;

namespace Welco.Shared.Infrastructure.ExchangeRate
{
    /// <summary>
    /// FastForex provider (https://api.fastforex.io). Live market quotes incl.
    /// MENA (EGP/DZD/SAR/AED). Quoted codes come from the database (via the
    /// service) — nothing hardcoded here. Requires ExchangeRateSettings:ApiKey.
    /// Select via ExchangeRateSettings:Provider = "FastForex".
    /// </summary>
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

        private string ApiKey =>
            _settings.ApiKey?.Trim() ?? throw new InvalidOperationException("FastForex ApiKey is not configured (ExchangeRateSettings:ApiKey)");

        /// <summary>Codes come from the database (via the service), never hardcoded.</summary>
        private static string ToList(IReadOnlyCollection<string>? targetCodes, string baseCode)
        {
            var codes = (targetCodes ?? [])
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c.Trim().ToUpperInvariant())
                .Where(c => c.Length == 3 && c != baseCode)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (codes.Count == 0)
                throw new InvalidOperationException("No target currencies in database to quote");
            return string.Join(",", codes);
        }

        public async Task<ExchangeRateResponse> GetLatestRatesAsync(string baseCurrency, IReadOnlyCollection<string>? targetCodes, CancellationToken cancellationToken)
        {
            var code = baseCurrency.Trim().ToUpperInvariant();
            var url = $"fetch-multi?from={Uri.EscapeDataString(code)}&to={ToList(targetCodes, code)}&api_key={Uri.EscapeDataString(ApiKey)}";
            _logger.LogInformation("Fetching rates from FastForex for base {Base}", code);
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadFromJsonAsync<FastForexResponse>(cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException($"FastForex returned empty for {code}");
            return ToResponse(data, code, null);
        }

        public async Task<ExchangeRateResponse?> GetHistoricalRatesAsync(string baseCurrency, DateOnly date, IReadOnlyCollection<string>? targetCodes, CancellationToken cancellationToken)
        {
            var code = baseCurrency.Trim().ToUpperInvariant();
            var url = $"historical?date={date:yyyy-MM-dd}&from={Uri.EscapeDataString(code)}&to={ToList(targetCodes, code)}&api_key={Uri.EscapeDataString(ApiKey)}";
            _logger.LogInformation("Fetching historical rates from FastForex: {Date} base {Base}", date.ToString("yyyy-MM-dd"), code);
            HttpResponseMessage response;
            try
            {
                response = await _httpClient.GetAsync(url, cancellationToken);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            if (!response.IsSuccessStatusCode) return null;
            var data = await response.Content.ReadFromJsonAsync<FastForexResponse>(cancellationToken: cancellationToken);
            if (data == null || data.Results == null || data.Results.Count == 0) return null;
            return ToResponse(data, code, date);
        }

        private ExchangeRateResponse ToResponse(FastForexResponse data, string requestedBase, DateOnly? forcedDate)
        {
            if (data.Results == null || data.Results.Count == 0)
                throw new InvalidOperationException($"FastForex returned empty rates for {requestedBase}");
            var dict = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in data.Results)
            {
                if (kv.Value <= 0) continue;
                var c = kv.Key.Trim().ToUpperInvariant();
                if (c.Length != 3 || c == requestedBase) continue;
                dict[c] = kv.Value;
            }
            if (dict.Count == 0)
                throw new InvalidOperationException("No valid rates parsed from FastForex response");
            DateOnly date;
            if (forcedDate.HasValue) date = forcedDate.Value;
            else if (!string.IsNullOrWhiteSpace(data.Updated) && DateTime.TryParse(data.Updated, out var dt)) date = DateOnly.FromDateTime(dt.Date);
            else if (!string.IsNullOrWhiteSpace(data.Date) && DateOnly.TryParse(data.Date, out var dd)) date = dd;
            else date = DateOnly.FromDateTime(DateTime.UtcNow.Date);
            return new ExchangeRateResponse
            {
                BaseCurrency = requestedBase,
                Date = date,
                Rates = dict,
                Source = ProviderName,
                FetchedAt = DateTime.UtcNow
            };
        }

        private sealed class FastForexResponse
        {
            [JsonPropertyName("base")] public string? Base { get; set; }
            [JsonPropertyName("results")] public Dictionary<string, decimal>? Results { get; set; }
            [JsonPropertyName("updated")] public string? Updated { get; set; }
            [JsonPropertyName("date")] public string? Date { get; set; }
        }
    }
}
