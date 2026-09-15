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
    public async Task GetLatestRates_UsesFetchOne_WithApiKey_AndParsesResultShape()
    {
        var handler = new StubHandler
        {
            Responder = req =>
            {
                var q = req.RequestUri!.Query;
                if (q.Contains("to=EGP")) return JsonResponse("{\"base\":\"USD\",\"result\":{\"EGP\":51.8158},\"updated\":\"2026-09-15T14:17:21Z\",\"ms\":4}");
                if (q.Contains("to=DZD")) return JsonResponse("{\"base\":\"USD\",\"result\":{\"DZD\":134.335},\"updated\":\"2026-09-15T14:17:21Z\",\"ms\":4}");
                return JsonResponse("{}", HttpStatusCode.NotFound);
            }
        };
        var provider = CreateProvider(handler);

        var res = await provider.GetLatestRatesAsync("USD", new[] { "EGP", "DZD" }, CancellationToken.None);

        Assert.Equal("FastForex", provider.ProviderName);
        Assert.Equal("USD", res.BaseCurrency);
        Assert.Equal(new DateOnly(2026, 9, 15), res.Date);
        Assert.Equal(51.8158m, res.Rates["EGP"]);
        Assert.Equal(134.335m, res.Rates["DZD"]);
        Assert.Equal("FastForex", res.Source);
        // fetch-one endpoint with api_key query param, no Bearer header
        foreach (var req in handler.Requests)
        {
            Assert.Contains("fetch-one", req.RequestUri!.ToString());
            Assert.Contains("api_key=test-key", req.RequestUri!.ToString());
            Assert.Null(req.Headers.Authorization);
        }
    }

    [Fact]
    public async Task Convert_UsesFetchOne_AndMultipliesAmountByRate()
    {
        var handler = new StubHandler
        {
            Response = JsonResponse("{\"base\":\"USD\",\"result\":{\"EGP\":51.8158},\"updated\":\"2026-09-15T14:17:21Z\",\"ms\":4}")
        };
        var provider = CreateProvider(handler);

        var res = await provider.ConvertAsync("USD", "EGP", 2m, CancellationToken.None);

        Assert.Equal(2m, res.Amount);
        Assert.Equal("USD", res.FromCurrency);
        Assert.Equal("EGP", res.ToCurrency);
        Assert.Equal(51.8158m, res.Rate);
        Assert.Equal(103.6316m, res.ConvertedAmount);
        Assert.Equal(new DateOnly(2026, 9, 15), res.RateDate);
        Assert.Equal("FastForex", res.Source);
        Assert.Equal(2, res.DecimalDigits);
        Assert.Contains("fetch-one", handler.LastRequest!.RequestUri!.ToString());
        Assert.Contains("api_key=test-key", handler.LastRequest!.RequestUri!.ToString());
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
}
