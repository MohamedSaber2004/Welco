using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Welco.Shared.Common.Options;
using Welco.Shared.Infrastructure.ExchangeRate;

namespace Welco.Tests;

public class YahooFinanceProviderTests
{
    private const string YahooBase = "https://query1.finance.yahoo.com/v8/finance/";
    private const string CdnBase = "https://cdn.jsdelivr.net/npm/@fawazahmed0/currency-api@latest/v1/currencies/";

    private static YahooFinanceProvider CreateProvider(StubHandler yahooHandler, StubHandler? cdnHandler = null)
    {
        var settings = new ExchangeRateSettings { BaseCurrency = "USD", TimeoutSeconds = 5, BaseUrl = CdnBase.TrimEnd('/') };
        var opts = Options.Create(settings);
        var cdn = new FawazahmedCdnProvider(
            new HttpClient(cdnHandler ?? new StubHandler()) { BaseAddress = new Uri(CdnBase) },
            opts,
            NullLogger<FawazahmedCdnProvider>.Instance);
        return new YahooFinanceProvider(
            new HttpClient(yahooHandler) { BaseAddress = new Uri(YahooBase) },
            cdn,
            opts,
            NullLogger<YahooFinanceProvider>.Instance);
    }

    private static HttpResponseMessage JsonResponse(string json, HttpStatusCode status = HttpStatusCode.OK)
        => new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    private static string ChartJson(string symbol, decimal price, long unixTime)
        => "{\"chart\":{\"result\":[{\"meta\":{\"currency\":\"EGP\",\"symbol\":\"" + symbol
            + "\",\"regularMarketPrice\":" + price.ToString(System.Globalization.CultureInfo.InvariantCulture)
            + ",\"regularMarketTime\":" + unixTime + "}}],\"error\":null}}";

    [Fact]
    public async Task Convert_DirectPair_ParsesLivePrice()
    {
        var handler = new StubHandler
        {
            Response = JsonResponse(ChartJson("USDEGP=X", 51.72m, 1790377146))
        };
        var provider = CreateProvider(handler);

        var res = await provider.ConvertAsync("USD", "EGP", 2m, CancellationToken.None);

        Assert.Equal("YahooFinance", provider.ProviderName);
        Assert.Equal(51.72m, res.Rate);
        Assert.Equal(103.44m, res.ConvertedAmount);
        Assert.Equal(DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(1790377146).UtcDateTime.Date), res.RateDate);
        Assert.Equal("YahooFinance", res.Source);
        Assert.Single(handler.Requests);
        Assert.Contains("chart/USDEGP%3DX", handler.LastRequest!.RequestUri!.ToString());
    }

    [Fact]
    public async Task Convert_DirectMissing_UsesInversePair()
    {
        var handler = new StubHandler
        {
            Responder = req => req.RequestUri!.ToString().Contains("USDJPY")
                ? JsonResponse("{}", HttpStatusCode.NotFound)
                : JsonResponse(ChartJson("JPYUSD=X", 0.0067m, 1790377146))
        };
        var provider = CreateProvider(handler);

        var res = await provider.ConvertAsync("USD", "JPY", 1m, CancellationToken.None);

        Assert.Equal("YahooFinance", res.Source);
        Assert.True(res.Rate > 100m); // 1 / 0.0067
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task Convert_YahooDown_FallsBackToCdnTable()
    {
        var yahooHandler = new StubHandler { Response = JsonResponse("{}", HttpStatusCode.NotFound) };
        var cdnHandler = new StubHandler
        {
            Response = JsonResponse("{\"date\":\"2026-09-25\",\"usd\":{\"egp\":48.1}}")
        };
        var provider = CreateProvider(yahooHandler, cdnHandler);

        var res = await provider.ConvertAsync("USD", "EGP", 2m, CancellationToken.None);

        Assert.Equal(48.1m, res.Rate);
        Assert.Equal(96.2m, res.ConvertedAmount);
        Assert.Equal("FawazahmedCDN", res.Source);
        Assert.Single(cdnHandler.Requests);
    }

    [Fact]
    public async Task Convert_SameCurrency_ReturnsIdentity_WithoutHttpCall()
    {
        var handler = new StubHandler { Response = JsonResponse("{}") };
        var provider = CreateProvider(handler);

        var res = await provider.ConvertAsync("USD", "USD", 5m, CancellationToken.None);

        Assert.Equal(1m, res.Rate);
        Assert.Equal(5m, res.ConvertedAmount);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task GetLatestRates_FetchesEachTargetSequentially()
    {
        var handler = new StubHandler
        {
            Responder = req =>
            {
                var url = req.RequestUri!.ToString();
                if (url.Contains("USDEGP")) return JsonResponse(ChartJson("USDEGP=X", 51.72m, 1790377146));
                if (url.Contains("USDDZD")) return JsonResponse(ChartJson("USDDZD=X", 130.5m, 1790377146));
                return JsonResponse("{}", HttpStatusCode.NotFound);
            }
        };
        var provider = CreateProvider(handler);

        var res = await provider.GetLatestRatesAsync("USD", new[] { "EGP", "DZD" }, CancellationToken.None);

        Assert.Equal("USD", res.BaseCurrency);
        Assert.Equal(51.72m, res.Rates["EGP"]);
        Assert.Equal(130.5m, res.Rates["DZD"]);
        Assert.Equal("YahooFinance", res.Source);
    }
}
