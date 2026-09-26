using System.Reflection;
using Welco.Shared.Common.Classes;
using Welco.Shared.Common.Helpers;
using Welco.Shared.Domain.Models;
using Welco.Shared.Enums;

namespace Welco.Tests;

/// <summary>
/// Covers the rule that decides which distributor application represents a
/// user's organization. Both failure modes here previously made an approved
/// provider render as "pending" (or as no organization at all):
///   1. the application was keyed by creator id, but the lookup only matched email
///   2. a later re-submission shadowed the already-approved application
/// </summary>
public sealed class DistributorApplicationResolverTests
{
    private const string UserId = "11111111-2222-3333-4444-555555555555";
    private const string UserEmail = "owner@mycompany.test";

    private static readonly DateTime T0 = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>CreatedAt has an internal setter, so seed it reflectively.</summary>
    private static void SetCreatedAt(DistributorApplication app, DateTime createdAt) =>
        typeof(BaseEntity)
            .GetProperty(nameof(BaseEntity.CreatedAt))!
            .SetValue(app, createdAt);

    private static DistributorApplication App(
        string createdBy,
        string contactEmail,
        DistributorApplicationStatus status,
        DateTime createdAt)
    {
        var app = new DistributorApplication
        {
            CompanyName = "My Company",
            ContactEmail = contactEmail,
            Type = CompanyType.Distributor,
            Status = status,
        };
        // Mirror what the create command does: stamps CreatedBy from the actor.
        app.MarkAsCreated(createdBy);
        SetCreatedAt(app, createdAt);
        return app;
    }

    [Fact]
    public void MatchesApplicationKeyedByCreatorUserId()
    {
        // The create command stores the creator's user id, not their email.
        var app = App(UserId, "sales@different-mailbox.test",
            DistributorApplicationStatus.Approved, T0);

        var resolved = DistributorApplicationResolver.ResolveForUser(
            new[] { app }, UserId, UserEmail);

        Assert.NotNull(resolved);
        Assert.Equal(app.Id, resolved!.Id);
    }

    [Fact]
    public void MatchesLegacyRowKeyedByCreatorEmail()
    {
        var app = App(UserEmail, "sales@different-mailbox.test",
            DistributorApplicationStatus.Approved, T0);

        var resolved = DistributorApplicationResolver.ResolveForUser(
            new[] { app }, UserId, UserEmail);

        Assert.NotNull(resolved);
    }

    [Fact]
    public void MatchesAnonymousApplicationByContactEmail()
    {
        // Submitted while signed out: CreatedBy is "System" and the only link
        // back to the account is the contact address.
        var app = App("System", UserEmail,
            DistributorApplicationStatus.Approved, T0);

        var resolved = DistributorApplicationResolver.ResolveForUser(
            new[] { app }, UserId, UserEmail);

        Assert.NotNull(resolved);
    }

    [Fact]
    public void ApprovedApplicationWinsOverNewerPendingResubmission()
    {
        var approved = App(UserId, UserEmail,
            DistributorApplicationStatus.Approved, T0);
        var newerPending = App(UserId, UserEmail,
            DistributorApplicationStatus.Pending, T0.AddDays(10));

        var resolved = DistributorApplicationResolver.ResolveForUser(
            new[] { newerPending, approved }, UserId, UserEmail);

        Assert.NotNull(resolved);
        Assert.Equal(approved.Id, resolved!.Id);
        Assert.Equal(DistributorApplicationStatus.Approved, resolved.Status);
    }

    [Fact]
    public void ApprovedApplicationWinsRegardlessOfInputOrder()
    {
        var approved = App(UserId, UserEmail,
            DistributorApplicationStatus.Approved, T0);
        var pending = App(UserId, UserEmail,
            DistributorApplicationStatus.Pending, T0.AddDays(1));

        var resolved = DistributorApplicationResolver.ResolveForUser(
            new[] { approved, pending }, UserId, UserEmail);

        Assert.NotNull(resolved);
        Assert.Equal(approved.Id, resolved!.Id);
    }

    [Fact]
    public void FallsBackToNewestWhenNothingIsApproved()
    {
        var rejected = App(UserId, UserEmail,
            DistributorApplicationStatus.Rejected, T0);
        var pending = App(UserId, UserEmail,
            DistributorApplicationStatus.Pending, T0.AddDays(5));

        var resolved = DistributorApplicationResolver.ResolveForUser(
            new[] { rejected, pending }, UserId, UserEmail);

        Assert.NotNull(resolved);
        Assert.Equal(pending.Id, resolved!.Id);
    }

    [Fact]
    public void IgnoresAnotherUsersApplication()
    {
        var other = App(Guid.NewGuid().ToString(), "someone@elsewhere.test",
            DistributorApplicationStatus.Approved, T0);

        var resolved = DistributorApplicationResolver.ResolveForUser(
            new[] { other }, UserId, UserEmail);

        Assert.Null(resolved);
    }

    [Fact]
    public void IgnoresSoftDeletedApplication()
    {
        var deleted = App(UserId, UserEmail,
            DistributorApplicationStatus.Approved, T0);
        deleted.MarkAsDeleted("System");

        var resolved = DistributorApplicationResolver.ResolveForUser(
            new[] { deleted }, UserId, UserEmail);

        Assert.Null(resolved);
    }

    [Fact]
    public void ReturnsNullForEmptyInput()
    {
        Assert.Null(DistributorApplicationResolver.ResolveForUser(
            Array.Empty<DistributorApplication>(), UserId, UserEmail));
    }
}
