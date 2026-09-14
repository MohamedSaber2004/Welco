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
    public HttpRequestMessage? LastRequest { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        return Task.FromResult(Response);
    }
}

public class FrankfurterProviderTests
{
    private static FrankfurterProvider CreateProvider(StubHandler handler)
    {
        var settings = new ExchangeRateSettings { BaseCurrency = "USD", TimeoutSeconds = 5 };
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.frankfurter.app/") };
        return new FrankfurterProvider(client, Options.Create(settings), NullLogger<FrankfurterProvider>.Instance);
    }

    private static HttpResponseMessage JsonResponse(string json, HttpStatusCode status = HttpStatusCode.OK)
        => new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    [Fact]
    public async Task GetLatestRates_ParsesFrankfurterShape()
    {
        var handler = new StubHandler
        {
            Response = JsonResponse("{\"amount\":1.0,\"base\":\"USD\",\"date\":\"2026-09-11\",\"rates\":{\"EUR\":0.86266,\"SAR\":3.75}}")
        };
        var provider = CreateProvider(handler);

        var res = await provider.GetLatestRatesAsync("USD", CancellationToken.None);

        Assert.Equal("Frankfurter", provider.ProviderName);
        Assert.Equal("USD", res.BaseCurrency);
        Assert.Equal(new DateOnly(2026, 9, 11), res.Date);
        Assert.Equal(0.86266m, res.Rates["EUR"]);
        Assert.Equal(3.75m, res.Rates["SAR"]);
        Assert.Equal("Frankfurter", res.Source);
    }

    [Fact]
    public async Task GetHistoricalRates_NotFound_ReturnsNull()
    {
        var handler = new StubHandler
        {
            Response = JsonResponse("{}", HttpStatusCode.NotFound)
        };
        var provider = CreateProvider(handler);

        var res = await provider.GetHistoricalRatesAsync("USD", new DateOnly(1999, 1, 1), CancellationToken.None);

        Assert.Null(res);
    }
}
