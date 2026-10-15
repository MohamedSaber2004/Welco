using Microsoft.EntityFrameworkCore;
using Welco.Shared.Domain.Models;
using Welco.Shared.Persistance;
using Xunit;

namespace Welco.Tests;

public class AuditLogDeduplicationTests
{
    private WelcoDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<WelcoDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new WelcoDbContext(options);
    }

    [Fact]
    public async Task SaveChangesAsync_CreatesExactlyOneAuditLogRow_PerEntity()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "testuser@welco.com",
            Email = "testuser@welco.com",
            FullName = "Test User"
        };
        await context.ApplicationUsers.AddAsync(user);

        // Act - call SaveChangesAsync
        var result = await context.SaveChangesAsync();

        // Assert
        var auditLogs = await context.AuditLogs.ToListAsync();
        Assert.Single(auditLogs);
        Assert.Equal("ApplicationUser", auditLogs[0].EntityName);
        Assert.Equal("Create", auditLogs[0].Action);
        Assert.Equal(user.Id.ToString(), auditLogs[0].EntityId);
    }

    [Fact]
    public async Task SaveChangesAsync_WithUserRefreshToken_CreatesExactlyOneAuditLogRow()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var userId = Guid.NewGuid();
        var refreshToken = UserRefreshToken.Create(userId, "token-123456", DateTime.UtcNow.AddDays(30));
        await context.UserRefreshTokens.AddAsync(refreshToken);

        // Act
        await context.SaveChangesAsync();

        // Assert
        var auditLogs = await context.AuditLogs.ToListAsync();
        Assert.Single(auditLogs);
        Assert.Equal("UserRefreshToken", auditLogs[0].EntityName);
        Assert.Equal("Create", auditLogs[0].Action);
        Assert.Equal(refreshToken.Id.ToString(), auditLogs[0].EntityId);
    }

    [Fact]
    public void SaveChanges_Synchronous_CreatesExactlyOneAuditLogRow()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var category = new Category
        {
            Id = Guid.NewGuid(),
            NameEn = "Dental Equipment",
            NameAr = "Dental Equipment Ar"
        };
        context.Categories.Add(category);

        // Act
        context.SaveChanges();

        // Assert
        var auditLogs = context.AuditLogs.ToList();
        Assert.Single(auditLogs);
        Assert.Equal("Category", auditLogs[0].EntityName);
        Assert.Equal("Create", auditLogs[0].Action);
    }
}