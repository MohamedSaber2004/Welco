using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Options;

namespace Welco.Shared.Infrastructure.ExchangeRate
{
    /// <summary>
    /// Real-time (intraday) rates from Yahoo Finance chart API
    /// (GET chart/{FROM}{TO}=X -&gt; meta.regularMarketPrice).
    /// Matches Google's current rates closely. No API key.
    /// If Yahoo fails for any pair, the whole call falls back to the
    /// Fawazahmed CDN daily table — both sources are real data, no static fallback.
    /// </summary>
    public class YahooFinanceProvider : IExchangeRateProvider
    {
        public const string DefaultBaseUrl = "https://query1.finance.yahoo.com/v8/finance";

        private readonly HttpClient _httpClient;
        private readonly FawazahmedCdnProvider _fallback;
        private readonly ILogger<YahooFinanceProvider> _logger;

        public string ProviderName => "YahooFinance";

        public YahooFinanceProvider(
            HttpClient httpClient,
            FawazahmedCdnProvider fallback,
            IOptions<ExchangeRateSettings> options,
            ILogger<YahooFinanceProvider> logger)
        {
            _httpClient = httpClient;
            _fallback = fallback;
            _logger = logger;
            var timeout = options.Value.TimeoutSeconds > 0 ? options.Value.TimeoutSeconds : 10;
            if (_httpClient.Timeout == System.Threading.Timeout.InfiniteTimeSpan)
                _httpClient.Timeout = TimeSpan.FromSeconds(timeout);
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

        private async Task<(decimal Rate, DateOnly RateDate)> FetchSingleRateAsync(string from, string to, CancellationToken ct)
        {
            // Direct pair first (e.g. USDEGP=X), then inverse (e.g. EGPUSD=X inverted).
            try
            {
                return await FetchSymbolAsync($"{from}{to}=X", ct);
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is InvalidOperationException)
            {
                _logger.LogInformation(ex, "Yahoo direct pair {From}{To}=X unavailable, trying inverse", from, to);
                var (invRate, date) = await FetchSymbolAsync($"{to}{from}=X", ct);
                if (invRate <= 0)
                    throw new InvalidOperationException($"Yahoo Finance returned invalid rate for {from}->{to}");
                return (1m / invRate, date);
            }
        }

        private async Task<(decimal Rate, DateOnly RateDate)> FetchSymbolAsync(string symbol, CancellationToken ct)
        {
            var url = $"chart/{Uri.EscapeDataString(symbol)}?interval=1d&range=1d";
            _logger.LogInformation("Yahoo Finance fetch {Symbol}", symbol);
            var response = await _httpClient.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();
            using var doc = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: ct)
                ?? throw new InvalidOperationException($"Yahoo Finance returned empty for {symbol}");
            return ParseChart(doc, symbol);
        }

        private static (decimal Rate, DateOnly RateDate) ParseChart(JsonDocument doc, string symbol)
        {
            var root = doc.RootElement;
            if (!root.TryGetProperty("chart", out var chart)
                || !chart.TryGetProperty("result", out var result)
                || result.ValueKind != JsonValueKind.Array
                || result.GetArrayLength() == 0)
                throw new InvalidOperationException($"Yahoo Finance missing chart result for {symbol}");
            var meta = result[0].GetProperty("meta");
            if (!meta.TryGetProperty("regularMarketPrice", out var priceProp)
                || priceProp.ValueKind != JsonValueKind.Number
                || !priceProp.TryGetDecimal(out var price)
                || price <= 0)
                throw new InvalidOperationException($"Yahoo Finance missing price for {symbol}");
            var date = DateOnly.FromDateTime(DateTime.UtcNow.Date);
            if (meta.TryGetProperty("regularMarketTime", out var timeProp)
                && timeProp.ValueKind == JsonValueKind.Number
                && timeProp.TryGetInt64(out var unix))
            {
                date = DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(unix).UtcDateTime.Date);
            }
            return (price, date);
        }

        public async Task<ExchangeRateResponse> GetLatestRatesAsync(string baseCurrency, IReadOnlyCollection<string>? targetCodes, CancellationToken cancellationToken)
        {
            var code = baseCurrency.Trim().ToUpperInvariant();
            var targets = NormalizeTargets(targetCodes, code);
            try
            {
                // Sequential on purpose: Yahoo throttles bursty parallel quote requests.
                var dict = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                var rateDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);
                foreach (var t in targets)
                {
                    var (rate, date) = await FetchSingleRateAsync(code, t, cancellationToken);
                    dict[t] = rate;
                    if (date > rateDate) rateDate = date;
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
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Yahoo Finance failed for base {Base}, falling back to CDN daily table", code);
                return await _fallback.GetLatestRatesAsync(code, targets, cancellationToken);
            }
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
            try
            {
                var (rate, rateDate) = await FetchSingleRateAsync(from, to, cancellationToken);
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
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Yahoo Finance failed for {From}->{To}, falling back to CDN daily table", from, to);
                return await _fallback.ConvertAsync(from, to, amount, cancellationToken);
            }
        }
    }
}
