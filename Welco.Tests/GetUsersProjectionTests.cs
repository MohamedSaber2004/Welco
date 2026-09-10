using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Domain.Models;
using Welco.Shared.Enums;
using Welco.Shared.Persistance;

namespace Welco.Tests;

public sealed class GetUsersProjectionTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly WelcoDbContext _db;

    public GetUsersProjectionTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<WelcoDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new WelcoDbContext(options, currentUserService: null);
        _db.Database.EnsureCreated();

        _db.ApplicationUsers.AddRange(
            new ApplicationUser
            {
                Id = Guid.NewGuid(),
                FullName = "Admin User",
                Email = "admin@welco.test",
                UserName = "admin@welco.test",
                UserType = UserType.Admin,
                Language = AppLanguage.En,
                IsActive = true,
                EmailConfirmed = true,
            },
            new ApplicationUser
            {
                Id = Guid.NewGuid(),
                FullName = "Legacy Customer",
                Email = "demo.customer01@welco.health",
                UserName = "demo.customer01@welco.health",
                UserType = UserType.OrganizationUser,
                Language = AppLanguage.En,
                IsActive = true,
                EmailConfirmed = true,
            });
        _db.SaveChanges();
        _db.ChangeTracker.Clear();

_db.Database.ExecuteSqlRaw(
            "UPDATE Users SET UserType = 'Customer' WHERE Email = 'demo.customer01@welco.health'");
        _db.ChangeTracker.Clear();
    }

        [Fact]
    public async Task GetUsersShape_WithLegacyCustomerRow_Succeeds()
    {
        var items = await _db.ApplicationUsers
            .Where(u => !u.IsDeleted)
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAt)
            .Skip(0)
            .Take(50)
            .Select(u => new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email ?? string.Empty,
                PhoneNumber = u.PhoneNumber,
                ProfilePictureName = u.ProfilePictureName,
                UserType = u.UserType,
                CompanyId = u.CompanyId,
                Language = u.Language,
                IsActive = u.IsActive,
                IsEmailConfirmed = u.EmailConfirmed,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
            })
            .ToListAsync();

        foreach (var item in items)
            item.Roles = new List<string> { item.UserType.ToString() };

        Assert.Equal(2, items.Count);
        var legacy = Assert.Single(items, i => i.Email == "demo.customer01@welco.health");

Assert.Equal(UserType.OrganizationUser, legacy.UserType);
        Assert.Equal(new List<string> { nameof(UserType.OrganizationUser) }, legacy.Roles);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}
