using Microsoft.EntityFrameworkCore;
using Product.Services.API.Features.Products.Queries.GetProducts;
using Welco.Shared.Common.Repositories.Implementation.Base;
using ProductEntity = Welco.Shared.Domain.Models.Product;
using Welco.Shared.Persistance;
using Xunit;

namespace Welco.Tests;

public class ProductSearchTests
{
    private static DbContextOptions<WelcoDbContext> CreateInMemoryOptions()
    {
        return new DbContextOptionsBuilder<WelcoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public void GetProductsQuery_ResolvesAliases_SearchTerm_Search_Q_Query()
    {
        var q1 = new GetProductsQuery { SearchTerm = "Kelly" };
        Assert.Equal("Kelly", q1.SearchTerm);

        var q2 = new GetProductsQuery { Search = "Kelly" };
        Assert.Equal("Kelly", q2.SearchTerm);

        var q3 = new GetProductsQuery { Q = "Kelly" };
        Assert.Equal("Kelly", q3.SearchTerm);

        var q4 = new GetProductsQuery { Query = "Kelly" };
        Assert.Equal("Kelly", q4.SearchTerm);

        // Explicit SearchTerm takes precedence
        var q5 = new GetProductsQuery { SearchTerm = "Kelly", Search = "Metzenbaum" };
        Assert.Equal("Kelly", q5.SearchTerm);
    }

    [Fact]
    public async Task SearchKelly_ReturnsOnlyKellyProducts_NeverMetzenbaumOrCastroviejo()
    {
        var options = CreateInMemoryOptions();
        using var db = new WelcoDbContext(options);

        var catId = Guid.NewGuid();

        var p1 = ProductEntity.Create("Kelly Hemostatic Clamps Straight 14cm", "ملقط كيلي مستقيم 14 سم", "WL-100065", "kelly-clamps",
            "Serrated jaws for selective clamping", 59m, 100, "Specs", "img1.jpg", "German Stainless Steel", 14m, null, catId, null, "test");
        var p2 = ProductEntity.Create("Metzenbaum Delicate Dissecting Scissors Curved 18cm", "مقص ميتزنباوم 18 سم", "WL-100064", "metzenbaum-scissors",
            "Curved dissecting blades", 185m, 50, "Specs", "img2.jpg", "Tungsten Carbide", 18m, null, catId, null, "test");
        var p3 = ProductEntity.Create("Castroviejo Micro Needle Holder Straight 14cm", "حامل إبر كاستروفيجو 14 سم", "WL-100055", "castroviejo-holder",
            "Precision spring-action needle holder", 320m, 20, "Specs", "img3.jpg", "Titanium", 14m, null, catId, null, "test");

        await db.Products.AddRangeAsync(p1, p2, p3);
        await db.SaveChangesAsync();

        var unitOfWork = new UnitOfWork(db);
        var handler = new GetProductsQueryHandler(unitOfWork);

        // 1. Search using SearchTerm alias
        var result1 = await handler.Handle(new GetProductsQuery { SearchTerm = "Kelly" }, CancellationToken.None);
        Assert.NotNull(result1.Data);
        Assert.Single(result1.Data);
        Assert.Equal("Kelly Hemostatic Clamps Straight 14cm", result1.Data[0].NameEn);

        // 2. Search using Search alias (as sent by query parameters like ?search=Kelly)
        var result2 = await handler.Handle(new GetProductsQuery { Search = "Kelly" }, CancellationToken.None);
        Assert.NotNull(result2.Data);
        Assert.Single(result2.Data);
        Assert.Equal("Kelly Hemostatic Clamps Straight 14cm", result2.Data[0].NameEn);

        // 3. Multi-word search (tokenized): "Kelly 14cm" matches Kelly Hemostatic Clamps Straight 14cm
        var result3 = await handler.Handle(new GetProductsQuery { Search = "Kelly 14cm" }, CancellationToken.None);
        Assert.NotNull(result3.Data);
        Assert.Single(result3.Data);
        Assert.Equal("Kelly Hemostatic Clamps Straight 14cm", result3.Data[0].NameEn);

        // 4. Searching Metzenbaum only returns Metzenbaum
        var result4 = await handler.Handle(new GetProductsQuery { Search = "Metzenbaum" }, CancellationToken.None);
        Assert.NotNull(result4.Data);
        Assert.Single(result4.Data);
        Assert.Equal("Metzenbaum Delicate Dissecting Scissors Curved 18cm", result4.Data[0].NameEn);

        // 5. Searching Castroviejo only returns Castroviejo
        var result5 = await handler.Handle(new GetProductsQuery { Search = "Castroviejo" }, CancellationToken.None);
        Assert.NotNull(result5.Data);
        Assert.Single(result5.Data);
        Assert.Equal("Castroviejo Micro Needle Holder Straight 14cm", result5.Data[0].NameEn);
    }
}
