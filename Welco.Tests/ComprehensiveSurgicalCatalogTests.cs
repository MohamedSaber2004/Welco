using Welco.Shared.Persistance.Seeding;
using Xunit;

namespace Welco.Tests;

public class ComprehensiveSurgicalCatalogTests
{
    [Fact]
    public void CatalogHas94UniqueItems()
    {
        var catalog = ComprehensiveSurgicalCatalog.Catalog94;
        Assert.Equal(94, catalog.Length);

        var distinctEn = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var distinctAr = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < catalog.Length; i++)
        {
            var item = catalog[i];
            Assert.False(string.IsNullOrWhiteSpace(item.NameEn), $"Item {i} NameEn is empty");
            Assert.False(string.IsNullOrWhiteSpace(item.NameAr), $"Item {i} NameAr is empty");
            Assert.True(item.Price > 0, $"Item {i} Price must be > 0");

            Assert.True(distinctEn.Add(item.NameEn), $"Duplicate NameEn at index {i}: '{item.NameEn}'");
            Assert.True(distinctAr.Add(item.NameAr), $"Duplicate NameAr at index {i}: '{item.NameAr}'");
        }

        Assert.Equal(94, distinctEn.Count);
        Assert.Equal(94, distinctAr.Count);
    }
}
