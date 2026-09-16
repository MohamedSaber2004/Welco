using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Options;
using Welco.Shared.Infrastructure.ExchangeRate;

namespace Welco.Tests;

public class FakeProvider : IExchangeRateProvider
{
    public string ProviderName => "Fake";
    public Dictionary<string, decimal> Rates { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ["EGP"] = 50.9467m,
        ["EUR"] = 0.85m,
        ["GBP"] = 0.78m,
        ["SAR"] = 3.75m,
        ["AED"] = 3.6725m,
        ["JPY"] = 110.5m
    };
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.Date);
    public bool ShouldFail { get; set; }

    public Task<ExchangeRateResponse> GetLatestRatesAsync(string baseCurrency, IReadOnlyCollection<string>? targetCodes, CancellationToken ct)
    {
        if (ShouldFail) throw new HttpRequestException("Provider unavailable");
        return Task.FromResult(new ExchangeRateResponse
        {
            BaseCurrency = baseCurrency.ToUpperInvariant(),
            Date = Date,
            Rates = new Dictionary<string, decimal>(Rates, StringComparer.OrdinalIgnoreCase),
            Source = ProviderName,
            FetchedAt = DateTime.UtcNow
        });
    }

    public Task<ConversionResult> ConvertAsync(string fromCurrency, string toCurrency, decimal amount, CancellationToken cancellationToken)
    {
        if (ShouldFail) throw new HttpRequestException("Provider unavailable");
        var from = fromCurrency.Trim().ToUpperInvariant();
        var to = toCurrency.Trim().ToUpperInvariant();
        if (from == to) return Task.FromResult(new ConversionResult { Amount = amount, FromCurrency = from, ToCurrency = to, Rate = 1m, ConvertedAmount = amount, RateDate = Date, Source = ProviderName, DecimalDigits = 2 });
        decimal rate;
        if (Rates.TryGetValue(to, out var toRate)) rate = toRate;
        else if (Rates.TryGetValue(from, out var fromRate)) rate = 1m / fromRate;
        else throw new InvalidOperationException($"No rate for {from}->{to}");
        return Task.FromResult(new ConversionResult { Amount = amount, FromCurrency = from, ToCurrency = to, Rate = rate, ConvertedAmount = amount * rate, RateDate = Date, Source = ProviderName, DecimalDigits = 2 });
    }
}

public class FakeProvider2 : IExchangeRateProvider
{
    public string ProviderName => "Fake2";
    public Dictionary<string, decimal> Rates { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.Date);
    public Task<ExchangeRateResponse> GetLatestRatesAsync(string baseCurrency, IReadOnlyCollection<string>? targetCodes, CancellationToken ct)
    {
        return Task.FromResult(new ExchangeRateResponse
        {
            BaseCurrency = baseCurrency.ToUpperInvariant(),
            Date = Date,
            Rates = new Dictionary<string, decimal>(Rates, StringComparer.OrdinalIgnoreCase),
            Source = ProviderName,
            FetchedAt = DateTime.UtcNow
        });
    }

    public Task<ConversionResult> ConvertAsync(string fromCurrency, string toCurrency, decimal amount, CancellationToken cancellationToken)
    {
        var from = fromCurrency.Trim().ToUpperInvariant();
        var to = toCurrency.Trim().ToUpperInvariant();
        if (from == to) return Task.FromResult(new ConversionResult { Amount = amount, FromCurrency = from, ToCurrency = to, Rate = 1m, ConvertedAmount = amount, RateDate = Date, Source = ProviderName, DecimalDigits = 2 });
        decimal rate;
        if (Rates.TryGetValue(to, out var toRate)) rate = toRate;
        else if (Rates.TryGetValue(from, out var fromRate)) rate = 1m / fromRate;
        else throw new InvalidOperationException($"No rate for {from}->{to}");
        return Task.FromResult(new ConversionResult { Amount = amount, FromCurrency = from, ToCurrency = to, Rate = rate, ConvertedAmount = amount * rate, RateDate = Date, Source = ProviderName, DecimalDigits = 2 });
    }
}

public class ExchangeRateServiceTests
{
    private readonly FakeProvider _provider;
    private readonly ExchangeRateService _svc;
    private readonly ExchangeRateSettings _settings = new() { BaseCurrency = "USD", TimeoutSeconds = 5 };

    public ExchangeRateServiceTests()
    {
        _provider = new FakeProvider();
        _svc = new ExchangeRateService(_provider, Options.Create(_settings), NullLogger<ExchangeRateService>.Instance);
    }

    [Fact] public async Task USD_USD_ReturnsSameAmount()
    {
        var r = await _svc.ConvertWithDetailsAsync(100m, "USD", "USD", CancellationToken.None);
        Assert.Equal(100m, r.ConvertedAmount);
        Assert.Equal(1m, r.Rate);
    }

    [Fact] public async Task USD_EGP_ConvertsCorrectly()
    {
        var r = await _svc.ConvertWithDetailsAsync(100m, "USD", "EGP", CancellationToken.None);
        Assert.Equal(5094.67m, r.ConvertedAmount);
        Assert.Equal(50.9467m, r.Rate);
    }

    [Fact] public async Task EGP_USD_ConvertsCorrectly()
    {
        var r = await _svc.ConvertWithDetailsAsync(50.9467m, "EGP", "USD", CancellationToken.None);
        Assert.InRange(r.ConvertedAmount, 0.99m, 1.01m);
    }

    [Fact] public async Task USD_EUR_ConvertsCorrectly()
    {
        var r = await _svc.ConvertWithDetailsAsync(100m, "USD", "EUR", CancellationToken.None);
        Assert.Equal(85m, r.ConvertedAmount);
        Assert.Equal(0.85m, r.Rate);
    }

    [Fact] public async Task InvalidCurrency_Throws()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => _svc.ConvertWithDetailsAsync(10m, "USD", "XXX", CancellationToken.None));
    }

    [Fact] public async Task ZeroAmount_ReturnsZero()
    {
        var r = await _svc.ConvertWithDetailsAsync(0m, "USD", "EGP", CancellationToken.None);
        Assert.Equal(0m, r.ConvertedAmount);
        Assert.Equal(50.9467m, r.Rate);
    }

    [Fact] public async Task NegativeAmount_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _svc.ConvertWithDetailsAsync(-5m, "USD", "EGP", CancellationToken.None));
    }

    [Fact] public async Task MissingRate_Throws()
    {
        _provider.Rates.Remove("EGP");
        await Assert.ThrowsAsync<InvalidOperationException>(() => _svc.ConvertWithDetailsAsync(10m, "USD", "EGP", CancellationToken.None));
    }

    [Fact] public async Task ConvertCartTotal_RealRatesNoCeiling()
    {
        var req = new ConvertCartTotalRequest
        {
            ToCurrency = "EGP",
            Lines = new List<CartTotalLineRequest>
            {
                new() { Key = "p1", UnitAmount = 310.8m, Quantity = 2, FromCurrency = "USD" },
                new() { Key = "p2", UnitAmount = 10.2m, Quantity = 3, FromCurrency = "USD" },
                new() { Key = "p3", UnitAmount = 10m, Quantity = 1, FromCurrency = "EGP" }
            }
        };
        var res = await _svc.ConvertCartTotalAsync(req, CancellationToken.None);
        Assert.Equal("EGP", res.ToCurrency);
        Assert.Equal(3, res.Lines.Count);
        Assert.Equal(310.8m, res.Lines[0].UnitAmount);
        Assert.Equal(50.9467m, res.Lines[0].Rate);
        Assert.Equal(310.8m * 50.9467m, res.Lines[0].ConvertedUnitAmount);
        Assert.Equal(310.8m * 50.9467m * 2, res.Lines[0].LineTotal);
        Assert.Equal(10.2m, res.Lines[1].UnitAmount);
        Assert.Equal(50.9467m, res.Lines[1].Rate);
        Assert.Equal(10.2m * 50.9467m, res.Lines[1].ConvertedUnitAmount);
        Assert.Equal(10.2m * 50.9467m * 3, res.Lines[1].LineTotal);
        Assert.Equal(10m, res.Lines[2].UnitAmount);
        Assert.Equal(1m, res.Lines[2].Rate);
        Assert.Equal(10m, res.Lines[2].ConvertedUnitAmount);
        Assert.Equal(10m, res.Lines[2].LineTotal);
        var expectedTotal = (310.8m * 50.9467m * 2) + (10.2m * 50.9467m * 3) + 10m;
        Assert.Equal(expectedTotal, res.Subtotal);
        Assert.Equal(expectedTotal, res.Total);
    }

    [Fact]
    public async Task GetLatestRates_ReturnsProviderRates()
    {
        var rates = await _svc.GetLatestRatesAsync("USD", CancellationToken.None);
        Assert.Contains(rates, r => r.TargetCurrency == "EGP");
        Assert.Equal(50.9467m, rates.First(r => r.TargetCurrency == "EGP").Rate);
    }

    [Fact]
    public async Task GetLatestRates_FetchesLive_EveryCall_NoCache()
    {
        var first = await _svc.GetLatestRatesAsync("USD", CancellationToken.None);
        _provider.Rates["EGP"] = 999m;
        var second = await _svc.GetLatestRatesAsync("USD", CancellationToken.None);
        // No cache: second call sees the live provider value
        Assert.Equal(999m, second.First(r => r.TargetCurrency == "EGP").Rate);
    }
}
