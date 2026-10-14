using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Common.Extensions;
using Welco.Shared.Domain.Models;
using Welco.Shared.Persistance;
using Xunit;
using Xunit.Abstractions;

namespace Welco.Tests;

public class LiveDatabaseProductTest
{
    private readonly ITestOutputHelper _output;

    public LiveDatabaseProductTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task TestProductQueryDirectly()
    {
        var options = new DbContextOptionsBuilder<WelcoDbContext>()
            .UseSqlServer("Server=db65360.public.databaseasp.net; Database=db65360; User Id=db65360; Password=Mo@123456; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;")
            .Options;

        using var db = new WelcoDbContext(options);

        var query = db.Products.Where(p => !p.IsDeleted);
        var totalCount = await query.CountAsync();
        _output.WriteLine($"Total count: {totalCount}");

        var rawEntities = await query.OrderByDescending(p => p.CreatedAt).Skip(0).Take(12).ToListAsync();
        _output.WriteLine($"Raw entities count: {rawEntities.Count}");

        var projectedQuery = query
            .OrderByDescending(p => p.CreatedAt)
            .Skip(0)
            .Take(12)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                NameEn = p.NameEn,
                NameAr = p.NameAr,
                Sku = p.Sku,
                Slug = p.Slug,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                Specifications = p.Specifications,
                ImageName = p.ImageName,
                Material = p.Material,
                LengthCm = p.LengthCm,
                CurrencyId = p.CurrencyId,
                CurrencyCode = p.Currency != null ? p.Currency.Code : null,
                CurrencySymbol = p.Currency != null ? p.Currency.Symbol : null,
                CategoryId = p.CategoryId,
                CategoryNameEn = p.Category != null ? p.Category.NameEn : null,
                CategoryNameAr = p.Category != null ? p.Category.NameAr : null,
                CompanyId = p.CompanyId,
                CompanyName = p.Company != null ? p.Company.Name : null,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            });

        _output.WriteLine("Projected SQL: " + projectedQuery.ToQueryString());

        var pagedItems = await projectedQuery.ToListAsync();
        _output.WriteLine($"Paged items count: {pagedItems.Count}");
    }

    [Fact]
    public async Task TestCategoryProductCount()
    {
        var options = new DbContextOptionsBuilder<WelcoDbContext>()
            .UseSqlServer("Server=db65360.public.databaseasp.net; Database=db65360; User Id=db65360; Password=Mo@123456; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;")
            .Options;

        using var db = new WelcoDbContext(options);

        var query = db.Categories.Where(c => !c.IsDeleted && c.ParentCategoryId == null)
            .Select(c => new
            {
                c.Id,
                c.NameEn,
                ProductCount = c.Products.Count(p => !p.IsDeleted) +
                               c.SubCategories.Where(sc => !sc.IsDeleted).SelectMany(sc => sc.Products).Count(p => !p.IsDeleted)
            });

        _output.WriteLine("Category SQL: " + query.ToQueryString());
        var list = await query.ToListAsync();
        foreach (var item in list)
        {
            _output.WriteLine($"{item.NameEn}: {item.ProductCount}");
        }
    }
}
