using Content.Services.API.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Domain.Models;
using Welco.Shared.Persistance;

namespace Welco.Tests;

public sealed class SupportTicketDateTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly WelcoDbContext _db;
    private readonly ApplicationUser _user;

    public SupportTicketDateTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<WelcoDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new WelcoDbContext(options, currentUserService: null);
        _db.Database.EnsureCreated();

        _user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            FullName = "Support User",
            Email = "support.user@welco.test",
            UserName = "support.user@welco.test"
        };
        _db.ApplicationUsers.Add(_user);
        _db.SaveChanges();
    }

    [Fact]
    public void SetReply_ClampsRepliedAt_WhenEarlierThanCreatedAt()
    {
        var ticket = new SupportTicket
        {
            Id = Guid.NewGuid(),
            UserId = _user.Id,
            Subject = "Test Issue",
            Message = "Test Body",
            Status = "Open",
        };
        ticket.MarkAsCreated("test-user");
        var createdAt = ticket.CreatedAt;

        // Try setting a reply date earlier than creation date (e.g., 3 days before)
        var earlyReply = createdAt.AddDays(-3);
        ticket.SetReply("Test reply", Guid.NewGuid(), earlyReply);

        Assert.Equal("Answered", ticket.Status);
        Assert.NotNull(ticket.RepliedAt);
        Assert.True(ticket.RepliedAt >= ticket.CreatedAt, "RepliedAt must never be earlier than CreatedAt");
        Assert.Equal(createdAt, ticket.RepliedAt);
    }

    [Fact]
    public void SetReply_PreservesRepliedAt_WhenLaterThanCreatedAt()
    {
        var ticket = new SupportTicket
        {
            Id = Guid.NewGuid(),
            UserId = _user.Id,
            Subject = "Test Issue",
            Message = "Test Body",
            Status = "Open",
        };
        ticket.MarkAsCreated("test-user");
        var createdAt = ticket.CreatedAt;

        var validReplyTime = createdAt.AddHours(4);
        ticket.SetReply("Official reply", Guid.NewGuid(), validReplyTime);

        Assert.Equal("Answered", ticket.Status);
        Assert.Equal(validReplyTime, ticket.RepliedAt);
    }

    [Fact]
    public async Task SupportTicketProjection_GuaranteesRepliedAtNotEarlierThanCreatedAt()
    {
        var created = new DateTime(2026, 9, 8, 12, 0, 0, DateTimeKind.Utc);
        var badReplied = new DateTime(2026, 9, 5, 12, 0, 0, DateTimeKind.Utc); // 3 days earlier

        var ticket = new SupportTicket
        {
            Id = Guid.NewGuid(),
            UserId = _user.Id,
            Subject = "Ticket Created 9/8 with bad reply date 9/5",
            Message = "My order issue",
            Status = "Answered",
            Reply = "We have investigated your request.",
            RepliedAt = badReplied
        };
        ticket.MarkAsCreated("test-seeder", created);

        _db.SupportTickets.Add(ticket);
        await _db.SaveChangesAsync();
        _db.ChangeTracker.Clear();

        var dto = await _db.SupportTickets
            .AsNoTracking()
            .Where(t => t.Id == ticket.Id)
            .Select(ContentDtoMapper.SupportTicketProjection)
            .FirstOrDefaultAsync();

        Assert.NotNull(dto);
        Assert.Equal(created, dto!.CreatedAt);
        Assert.NotNull(dto.RepliedAt);
        // Guaranteed not earlier than CreatedAt
        Assert.True(dto.RepliedAt >= dto.CreatedAt, $"Projected RepliedAt ({dto.RepliedAt}) cannot be before CreatedAt ({dto.CreatedAt})");
        Assert.Equal(created, dto.RepliedAt);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}