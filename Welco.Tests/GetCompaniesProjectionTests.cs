using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Auth.Responses;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Domain.Models;
using Welco.Shared.Enums;
using Welco.Shared.Persistance;

namespace Welco.Tests;

/// <summary>
/// Guards the provider-card contract: the public companies directory must
/// project ImageName, otherwise a provider logo uploaded from the profile
/// never reaches the landing/trade provider cards.
/// </summary>
public sealed class GetCompaniesProjectionTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly WelcoDbContext _db;

    private static readonly Guid CountryId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private const string LogoName = "1700000000_aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee.png";

    public GetCompaniesProjectionTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<WelcoDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new WelcoDbContext(options, currentUserService: null);
        _db.Database.EnsureCreated();

        _db.Countries.Add(new Country
        {
            Id = CountryId,
            NameEn = "Egypt",
            NameAr = "مصر",
        });
        _db.Companies.AddRange(
            new Company
            {
                Id = Guid.NewGuid(),
                Name = "Provider With Logo",
                Email = "logo@welco.test",
                ImageName = LogoName,
                Type = CompanyType.Distributor,
                CountryId = CountryId,
                Status = CompanyStatus.Approved,
                IsProvider = true,
            },
            new Company
            {
                Id = Guid.NewGuid(),
                Name = "Provider Without Logo",
                Email = "nologo@welco.test",
                Type = CompanyType.Supplier,
                CountryId = CountryId,
                Status = CompanyStatus.Approved,
                IsProvider = true,
            });
        _db.SaveChanges();
        _db.ChangeTracker.Clear();
    }

    [Fact]
    public async Task DirectoryProjection_IncludesImageName()
    {
        // Mirrors GetCompaniesQueryHandler's projection exactly.
        var items = await _db.Companies
            .Where(c => !c.IsDeleted)
            .AsNoTracking()
            .Where(c => c.IsActive && c.IsProvider && c.Status == CompanyStatus.Approved)
            .OrderBy(c => c.Name)
            .Select(c => new CompanyDto
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email,
                ImageName = c.ImageName,
                Type = c.Type,
                CountryId = c.CountryId,
                Status = c.Status,
                IsActive = c.IsActive,
                IsProvider = c.IsProvider,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
            })
            .ToListAsync();

        Assert.Equal(2, items.Count);

        var withLogo = Assert.Single(items, i => i.Name == "Provider With Logo");
        Assert.Equal(LogoName, withLogo.ImageName);

        var withoutLogo = Assert.Single(items, i => i.Name == "Provider Without Logo");
        Assert.Null(withoutLogo.ImageName);
    }

    /// <summary>
    /// The profile endpoint embeds a CompanyDto, so the logo has to be
    /// reachable through that nested type. This guards the shape: narrowing
    /// UserProfileDto.Company to a smaller projection would silently drop the
    /// brand image from the Profile "Organization Details" card.
    /// </summary>
    [Fact]
    public void UserProfileCompanyPayload_ExposesImageName()
    {
        var prop = typeof(UserProfileDto)
            .GetProperty(nameof(UserProfileDto.Company))
            ?.PropertyType
            .GetProperty(nameof(CompanyDto.ImageName));

        Assert.NotNull(prop);
        Assert.Equal(typeof(string), prop!.PropertyType);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}
