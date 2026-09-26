using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Welco.Shared.Common.Options;
using Welco.Shared.Infrastructure.ExchangeRate;

namespace Welco.Tests;

public class StubHandler : HttpMessageHandler
{
    public HttpResponseMessage Response { get; set; } = new(HttpStatusCode.OK);
    public Func<HttpRequestMessage, HttpResponseMessage>? Responder { get; set; }
    public List<HttpRequestMessage> Requests { get; } = new();
    public HttpRequestMessage? LastRequest => Requests.Count > 0 ? Requests[^1] : null;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);
        if (Responder != null) return Task.FromResult(Responder(request));
        return Task.FromResult(Response);
    }
}

public class FawazahmedCdnProviderTests
{
    private const string CdnBase = "https://cdn.jsdelivr.net/npm/@fawazahmed0/currency-api@latest/v1/currencies/";

    private static FawazahmedCdnProvider CreateProvider(StubHandler handler)
    {
        var settings = new ExchangeRateSettings { BaseCurrency = "USD", TimeoutSeconds = 5, BaseUrl = CdnBase.TrimEnd('/') };
        var client = new HttpClient(handler) { BaseAddress = new Uri(CdnBase) };
        return new FawazahmedCdnProvider(client, Options.Create(settings), NullLogger<FawazahmedCdnProvider>.Instance);
    }

    private static HttpResponseMessage JsonResponse(string json, HttpStatusCode status = HttpStatusCode.OK)
        => new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    [Fact]
    public async Task GetLatestRates_FetchesBaseTable_AndFiltersTargets()
    {
        var handler = new StubHandler
        {
            Response = JsonResponse("{\"date\":\"2026-09-25\",\"usd\":{\"egp\":48.1,\"dzd\":134.2,\"eur\":0.92}}")
        };
        var provider = CreateProvider(handler);

        var res = await provider.GetLatestRatesAsync("USD", new[] { "EGP", "DZD" }, CancellationToken.None);

        Assert.Equal("FawazahmedCDN", provider.ProviderName);
        Assert.Equal("USD", res.BaseCurrency);
        Assert.Equal(new DateOnly(2026, 9, 25), res.Date);
        Assert.Equal(48.1m, res.Rates["EGP"]);
        Assert.Equal(134.2m, res.Rates["DZD"]);
        Assert.Equal("FawazahmedCDN", res.Source);
        // single CDN table fetch per call, lowercase file name
        Assert.Single(handler.Requests);
        Assert.EndsWith("usd.json", handler.LastRequest!.RequestUri!.ToString());
        Assert.Null(handler.LastRequest.Headers.Authorization);
    }

    [Fact]
    public async Task Convert_FetchesFromTable_AndMultipliesAmountByRate()
    {
        var handler = new StubHandler
        {
            Response = JsonResponse("{\"date\":\"2026-09-25\",\"usd\":{\"egp\":48.1}}")
        };
        var provider = CreateProvider(handler);

        var res = await provider.ConvertAsync("USD", "EGP", 2m, CancellationToken.None);

        Assert.Equal(2m, res.Amount);
        Assert.Equal("USD", res.FromCurrency);
        Assert.Equal("EGP", res.ToCurrency);
        Assert.Equal(48.1m, res.Rate);
        Assert.Equal(96.2m, res.ConvertedAmount);
        Assert.Equal(new DateOnly(2026, 9, 25), res.RateDate);
        Assert.Equal("FawazahmedCDN", res.Source);
        Assert.Equal(2, res.DecimalDigits);
        Assert.EndsWith("usd.json", handler.LastRequest!.RequestUri!.ToString());
    }

    [Fact]
    public async Task Convert_SameCurrency_ReturnsIdentity_WithoutHttpCall()
    {
        var handler = new StubHandler
        {
            Response = JsonResponse("{}")
        };
        var provider = CreateProvider(handler);

        var res = await provider.ConvertAsync("USD", "USD", 5m, CancellationToken.None);

        Assert.Equal(1m, res.Rate);
        Assert.Equal(5m, res.ConvertedAmount);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task GetLatestRates_MissingTarget_Throws()
    {
        var handler = new StubHandler
        {
            Response = JsonResponse("{\"date\":\"2026-09-25\",\"usd\":{\"egp\":48.1}}")
        };
        var provider = CreateProvider(handler);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => provider.GetLatestRatesAsync("USD", new[] { "XXX" }, CancellationToken.None));
    }
}
