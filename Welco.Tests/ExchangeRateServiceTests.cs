using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Options;
using Welco.Shared.Domain.Models;
using Welco.Shared.Infrastructure.ExchangeRate;
using Welco.Shared.Persistance;

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

    public Task<ExchangeRateResponse> GetLatestRatesAsync(string baseCurrency, CancellationToken ct)
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

    public Task<ExchangeRateResponse?> GetHistoricalRatesAsync(string baseCurrency, DateOnly date, CancellationToken ct)
    {
        if (ShouldFail) throw new HttpRequestException("Provider unavailable");
        return Task.FromResult<ExchangeRateResponse?>(new ExchangeRateResponse
        {
            BaseCurrency = baseCurrency.ToUpperInvariant(),
            Date = date,
            Rates = new Dictionary<string, decimal>(Rates, StringComparer.OrdinalIgnoreCase),
            Source = ProviderName,
            FetchedAt = DateTime.UtcNow
        });
    }
}

public class ExchangeRateServiceTests : IDisposable
{
    private readonly WelcoDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly FakeProvider _provider;
    private readonly ExchangeRateService _svc;
    private readonly ExchangeRateSettings _settings = new() { BaseCurrency = "USD", CacheExpirationMinutes = 5, TimeoutSeconds = 5 };

    public ExchangeRateServiceTests()
    {
        var opts = new DbContextOptionsBuilder<WelcoDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)).Options;
        _db = new WelcoDbContext(opts, null);
        // Seed USD + target currencies
        var usd = Currency.Create("US Dollar", "دولار أمريكي", "USD", "$", "Test", "$", 2);
        var egp = Currency.Create("Egyptian Pound", "جنيه مصري", "EGP", "E£", "Test", "E£", 2);
        var eur = Currency.Create("Euro", "يورو", "EUR", "€", "Test", "€", 2);
        var gbp = Currency.Create("British Pound", "جنيه إسترليني", "GBP", "£", "Test", "£", 2);
        var sar = Currency.Create("Saudi Riyal", "ريال سعودي", "SAR", "﷼", "Test", "﷼", 2);
        var jpy = Currency.Create("Japanese Yen", "ين ياباني", "JPY", "¥", "Test", "¥", 0);
        var aed = Currency.Create("UAE Dirham", "درهم إماراتي", "AED", "AED", "Test", "د.إ", 2);
        _db.Currencies.AddRange(usd, egp, eur, gbp, sar, jpy, aed);
        _db.SaveChanges();

        _cache = new MemoryCache(new MemoryCacheOptions());
        _provider = new FakeProvider();
        _svc = new ExchangeRateService(_db, _provider, Options.Create(_settings), _cache, NullLogger<ExchangeRateService>.Instance);
    }

    public void Dispose()
    {
        _db.Dispose();
        _cache.Dispose();
    }

    private async Task SeedRatesAsync(DateOnly? date = null)
    {
        date ??= DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var res = await _svc.SyncLatestRatesAsync(CancellationToken.None);
        if (!res.Success) throw new Exception("Seed failed: " + res.ErrorMessage);
    }

    [Fact] public async Task USD_USD_ReturnsSameAmount()
    {
        var r = await _svc.ConvertWithDetailsAsync(100m, "USD", "USD", CancellationToken.None);
        Assert.Equal(100m, r.ConvertedAmount);
        Assert.Equal(1m, r.Rate);
    }

    [Fact] public async Task USD_EGP_ConvertsCorrectly()
    {
        await SeedRatesAsync();
        var r = await _svc.ConvertWithDetailsAsync(100m, "USD", "EGP", CancellationToken.None);
        Assert.Equal(5094.67m, r.ConvertedAmount); // 100 * 50.9467
        Assert.Equal(50.9467m, r.Rate);
    }

    [Fact] public async Task EGP_USD_ConvertsCorrectly()
    {
        await SeedRatesAsync();
        var r = await _svc.ConvertWithDetailsAsync(5094.67m, "EGP", "USD", CancellationToken.None);
        // 5094.67 * (1 / 50.9467) ≈ 100
        Assert.InRange(r.ConvertedAmount, 99.99m, 100.01m);
    }

    [Fact] public async Task EUR_EGP_UsesCrossRate()
    {
        await SeedRatesAsync();
        var r = await _svc.ConvertWithDetailsAsync(100m, "EUR", "EGP", CancellationToken.None);
        var expectedRate = 50.9467m / 0.85m;
        Assert.InRange(r.Rate, expectedRate - 0.0001m, expectedRate + 0.0001m);
        Assert.InRange(r.ConvertedAmount, 5993m, 5994m); // 100 * 59.937...
    }

    [Fact] public async Task EGP_EUR_ReverseCross()
    {
        await SeedRatesAsync();
        var r = await _svc.ConvertWithDetailsAsync(5993.73m, "EGP", "EUR", CancellationToken.None);
        Assert.InRange(r.ConvertedAmount, 99.9m, 100.1m);
    }

    [Fact] public async Task InvalidCurrency_Throws()
    {
        await SeedRatesAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(() => _svc.ConvertWithDetailsAsync(10m, "USD", "XXX", CancellationToken.None));
    }

    [Fact] public async Task ZeroAmount_ReturnsZero()
    {
        await SeedRatesAsync();
        var r = await _svc.ConvertWithDetailsAsync(0m, "USD", "EGP", CancellationToken.None);
        Assert.Equal(0m, r.ConvertedAmount);
        Assert.Equal(50.9467m, r.Rate);
    }

    [Fact] public async Task NegativeAmount_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _svc.ConvertWithDetailsAsync(-5m, "USD", "EGP", CancellationToken.None));
    }

    [Fact] public async Task MissingExchangeRate_Throws()
    {
        // No seed, clear DB
        _db.ExchangeRates.RemoveRange(_db.ExchangeRates);
        await _db.SaveChangesAsync();
        _provider.Rates.Remove("EGP");
        _cache.Remove($"exchange-rates:USD:{DateOnly.FromDateTime(DateTime.UtcNow.Date):yyyy-MM-dd}");
        // Now EGP missing, should throw after sync tries but filtered? Actually provider has no EGP, so conversion should fail
        await Assert.ThrowsAsync<InvalidOperationException>(() => _svc.ConvertWithDetailsAsync(10m, "USD", "EGP", CancellationToken.None));
    }

    [Fact] public async Task ProviderFailure_FallsBackToDb()
    {
        await SeedRatesAsync();
        _provider.ShouldFail = true;
        // Should serve from DB/cache, not throw
        var r = await _svc.ConvertWithDetailsAsync(10m, "USD", "EGP", CancellationToken.None);
        Assert.Equal(509.47m, r.ConvertedAmount); // 10 * 50.9467 = 509.467 -> 509.47
    }

    [Fact] public async Task CachedRate_Used()
    {
        await SeedRatesAsync();
        var r1 = await _svc.ConvertWithDetailsAsync(10m, "USD", "EUR", CancellationToken.None);
        // Change provider rate but cache should still return old
        _provider.Rates["EUR"] = 0.90m;
        var r2 = await _svc.ConvertWithDetailsAsync(10m, "USD", "EUR", CancellationToken.None);
        Assert.Equal(r1.Rate, r2.Rate);
    }

    [Fact] public async Task HistoricalRate_Retrieved()
    {
        var histDate = new DateOnly(2023, 1, 1);
        var res = await _svc.SyncHistoricalRatesAsync(histDate, CancellationToken.None);
        Assert.True(res.Success);
        var rates = await _svc.GetHistoricalRatesAsync("USD", histDate, CancellationToken.None);
        Assert.NotEmpty(rates);
    }

    [Fact] public async Task LatestRate_ReturnsToday()
    {
        await SeedRatesAsync();
        var rates = await _svc.GetLatestRatesAsync("USD", CancellationToken.None);
        Assert.Contains(rates, x => x.TargetCurrency == "EGP" && x.Rate == 50.9467m);
    }

    [Fact] public async Task RatePrecision_DecimalNotFloat()
    {
        await SeedRatesAsync();
        var r = await _svc.ConvertWithDetailsAsync(1m, "USD", "EGP", CancellationToken.None);
        Assert.IsType<decimal>(r.Rate);
        Assert.IsType<decimal>(r.ConvertedAmount);
        Assert.Equal(50.9467m, r.Rate);
    }

    [Fact] public async Task SameDaySync_Idempotent()
    {
        var r1 = await _svc.SyncLatestRatesAsync(CancellationToken.None);
        var r2 = await _svc.SyncLatestRatesAsync(CancellationToken.None);
        Assert.True(r1.Success && r2.Success);
        var count = await _db.ExchangeRates.CountAsync();
        // Should not duplicate unique (Base,Target,Date)
        Assert.Equal(r1.RatesCount, r2.RatesCount);
    }

    [Fact] public async Task DuplicateSync_NoDuplicateRows()
    {
        await SeedRatesAsync();
        var before = await _db.ExchangeRates.CountAsync();
        await _svc.SyncLatestRatesAsync(CancellationToken.None);
        var after = await _db.ExchangeRates.CountAsync();
        Assert.Equal(before, after);
    }

    [Fact]
    public async Task SyncLatestRates_SetsLogSuccessAndRatesCount()
    {
        var res = await _svc.SyncLatestRatesAsync(CancellationToken.None);
        Assert.True(res.Success);
        Assert.True(res.RatesCount > 0);

        var log = await _db.ExchangeRateSyncLogs.OrderByDescending(l => l.StartedAt).FirstOrDefaultAsync();
        Assert.NotNull(log);
        Assert.Equal(ExchangeRateSyncStatus.Success, log.Status);
        Assert.Equal(res.RatesCount, log.RatesCount);
        Assert.Equal("USD", log.BaseCurrency);
    }

    [Fact]
    public async Task SyncLatestRates_WhenProviderFails_SetsLogFailed()
    {
        _provider.ShouldFail = true;
        var res = await _svc.SyncLatestRatesAsync(CancellationToken.None);
        Assert.False(res.Success);

        var log = await _db.ExchangeRateSyncLogs.OrderByDescending(l => l.StartedAt).FirstOrDefaultAsync();
        Assert.NotNull(log);
        Assert.Equal(ExchangeRateSyncStatus.Failed, log.Status);
        Assert.False(string.IsNullOrWhiteSpace(log.ErrorMessage));
    }
}
