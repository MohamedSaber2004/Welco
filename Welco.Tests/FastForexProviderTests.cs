using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Welco.Shared.Common.Options;
using Welco.Shared.Infrastructure.ExchangeRate;

namespace Welco.Tests;

public class FastForexProviderTests
{
    private static FastForexProvider CreateProvider(StubHandler handler, string apiKey = "test-key")
    {
        var settings = new ExchangeRateSettings { BaseCurrency = "USD", TimeoutSeconds = 5, ApiKey = apiKey };
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.fastforex.io/") };
        return new FastForexProvider(client, Options.Create(settings), NullLogger<FastForexProvider>.Instance);
    }

    private static HttpResponseMessage JsonResponse(string json, HttpStatusCode status = HttpStatusCode.OK)
        => new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    [Fact]
    public async Task GetLatestRates_ParsesFetchMultiShape()
    {
        var handler = new StubHandler
        {
            Response = JsonResponse("{\"base\":\"USD\",\"results\":{\"EGP\":51.7276,\"DZD\":134.335},\"updated\":\"2026-09-14T13:20:39Z\",\"ms\":4}")
        };
        var provider = CreateProvider(handler);

        var res = await provider.GetLatestRatesAsync("USD", new[] { "EGP", "DZD" }, CancellationToken.None);

        Assert.Equal("FastForex", provider.ProviderName);
        Assert.Equal("USD", res.BaseCurrency);
        Assert.Equal(new DateOnly(2026, 9, 14), res.Date);
        Assert.Equal(51.7276m, res.Rates["EGP"]);
        Assert.Equal(134.335m, res.Rates["DZD"]);
        Assert.Equal("FastForex", res.Source);
        Assert.Contains("api_key=test-key", handler.LastRequest!.RequestUri!.ToString());
        Assert.Contains("to=EGP", handler.LastRequest!.RequestUri!.ToString());
    }

    [Fact]
    public async Task GetHistoricalRates_ParsesHistoricalShape()
    {
        var handler = new StubHandler
        {
            Response = JsonResponse("{\"date\":\"2026-09-13\",\"base\":\"USD\",\"results\":{\"EGP\":51.4},\"ms\":3}")
        };
        var provider = CreateProvider(handler);

        var res = await provider.GetHistoricalRatesAsync("USD", new DateOnly(2026, 9, 13), new[] { "EGP" }, CancellationToken.None);

        Assert.NotNull(res);
        Assert.Equal(new DateOnly(2026, 9, 13), res!.Date);
        Assert.Equal(51.4m, res.Rates["EGP"]);
    }

    [Fact]
    public async Task GetHistoricalRates_NotFound_ReturnsNull()
    {
        var handler = new StubHandler
        {
            Response = JsonResponse("{}", HttpStatusCode.NotFound)
        };
        var provider = CreateProvider(handler);

        var res = await provider.GetHistoricalRatesAsync("USD", new DateOnly(1999, 1, 1), new[] { "EGP" }, CancellationToken.None);

        Assert.Null(res);
    }
}
