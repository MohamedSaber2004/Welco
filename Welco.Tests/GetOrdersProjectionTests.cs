using Commerce.Services.API.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Domain.Models;
using Welco.Shared.Persistance;

namespace Welco.Tests;

public sealed class GetOrdersProjectionTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly WelcoDbContext _db;

    private static readonly Guid UsdCurrencyId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public GetOrdersProjectionTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<WelcoDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new WelcoDbContext(options, currentUserService: null);
        _db.Database.EnsureCreated();

        var usd = new Currency
        {
            Id = UsdCurrencyId,
            Code = "USD",
            NameEn = "US Dollar",
            NameAr = "USD",
            Symbol = "$",
            SymbolNative = "$",
        };
        _db.Currencies.Add(usd);

        _db.Orders.AddRange(
            new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = "ORD-TEST-001",
                CurrencyId = UsdCurrencyId,
                SnapshotBaseCurrency = "USD",
                SnapshotCurrencyCode = "USD",
                TotalAmount = 500.00m,
            },
            new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = "ORD-TEST-002",
                CurrencyId = null,
                SnapshotCurrencyCode = "EUR",
                TotalAmount = 250.00m,
            },
            new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = "ORD-TEST-003",
                CurrencyId = null,
                SnapshotCurrencyCode = null,
                TotalAmount = 100.00m,
            });
        _db.SaveChanges();
        _db.ChangeTracker.Clear();
    }

    [Fact]
    public async Task OrderProjection_PopulatesCurrencyCodeAndSymbol()
    {
        var orders = await _db.Orders
            .Where(o => !o.IsDeleted)
            .AsNoTracking()
            .Select(CommerceDtoMapper.OrderProjection)
            .ToListAsync();

        Assert.Equal(3, orders.Count);

        var order1 = Assert.Single(orders, o => o.OrderNumber == "ORD-TEST-001");
        Assert.Equal("USD", order1.CurrencyCode);
        Assert.Equal("$", order1.CurrencySymbol);

        var order2 = Assert.Single(orders, o => o.OrderNumber == "ORD-TEST-002");
        Assert.Equal("EUR", order2.CurrencyCode);

        var order3 = Assert.Single(orders, o => o.OrderNumber == "ORD-TEST-003");
        Assert.Equal("USD", order3.CurrencyCode);
        Assert.Equal("$", order3.CurrencySymbol);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}
