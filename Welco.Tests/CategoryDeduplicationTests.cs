using Microsoft.EntityFrameworkCore;
using Product.Services.API.Features.Categories.Commands.CreateCategory;
using Product.Services.API.Features.Categories.Commands.UpdateCategory;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Implementation.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Persistance;
using Xunit;

namespace Welco.Tests;

public class CategoryDeduplicationTests
{
    private class FakeCurrentUserService : ICurrentUserService
    {
        public Guid UserId { get; set; } = Guid.NewGuid();
        public string? Email { get; set; } = "admin@welco.com";
        public bool IsAuthenticated { get; set; } = true;
        public string? IpAddress { get; set; } = "127.0.0.1";
    }

    private static WelcoDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<WelcoDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new WelcoDbContext(options);
    }

    [Fact]
    public async Task CreateCategory_DuplicateEnglishNameUnderSameParent_ReturnsBadRequest()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);
        var uow = new UnitOfWork(context);
        var currentUser = new FakeCurrentUserService();

        var parent = Category.Create("Dental", "طب الأسنان", null, null, null, "admin");
        context.Categories.Add(parent);

        var existingChild = Category.Create("Orthodontic", "تقويم الأسنان", null, null, parent.Id, "admin");
        context.Categories.Add(existingChild);
        await context.SaveChangesAsync();

        var handler = new CreateCategoryCommandHandler(uow, currentUser);
        var command = new CreateCategoryCommand
        {
            NameEn = "orthodontic ",
            NameAr = "تقويم مختلف",
            ParentCategoryId = parent.Id
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(LocalizationKeys.Category.AlreadyExists, result.Message);
    }

    [Fact]
    public async Task CreateCategory_DuplicateArabicNameUnderSameParent_ReturnsBadRequest()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);
        var uow = new UnitOfWork(context);
        var currentUser = new FakeCurrentUserService();

        var parent = Category.Create("Dental", "طب الأسنان", null, null, null, "admin");
        context.Categories.Add(parent);

        var existingChild = Category.Create("Orthodontic", "تقويم الأسنان", null, null, parent.Id, "admin");
        context.Categories.Add(existingChild);
        await context.SaveChangesAsync();

        var handler = new CreateCategoryCommandHandler(uow, currentUser);
        var command = new CreateCategoryCommand
        {
            NameEn = "Braces",
            NameAr = " تقويم الأسنان ",
            ParentCategoryId = parent.Id
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(LocalizationKeys.Category.AlreadyExists, result.Message);
    }

    [Fact]
    public async Task CreateCategory_DuplicateAtRootLevel_ReturnsBadRequest()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);
        var uow = new UnitOfWork(context);
        var currentUser = new FakeCurrentUserService();

        var root = Category.Create("Dental", "طب الأسنان", null, null, null, "admin");
        context.Categories.Add(root);
        await context.SaveChangesAsync();

        var handler = new CreateCategoryCommandHandler(uow, currentUser);
        var command = new CreateCategoryCommand
        {
            NameEn = "dental",
            NameAr = "أسنان أخرى",
            ParentCategoryId = null
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(LocalizationKeys.Category.AlreadyExists, result.Message);
    }

    [Fact]
    public async Task CreateCategory_SameNameUnderDifferentParent_Succeeds()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);
        var uow = new UnitOfWork(context);
        var currentUser = new FakeCurrentUserService();

        var parent1 = Category.Create("Dental", "طب الأسنان", null, null, null, "admin");
        var parent2 = Category.Create("Surgical", "جراحة", null, null, null, "admin");
        context.Categories.AddRange(parent1, parent2);

        var child1 = Category.Create("Forceps", "ملقط", null, null, parent1.Id, "admin");
        context.Categories.Add(child1);
        await context.SaveChangesAsync();

        var handler = new CreateCategoryCommandHandler(uow, currentUser);
        var command = new CreateCategoryCommand
        {
            NameEn = "Forceps",
            NameAr = "ملقط",
            ParentCategoryId = parent2.Id
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Forceps", result.Data.NameEn);
    }

    [Fact]
    public async Task UpdateCategory_SameEntityWithoutNameChange_Succeeds()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);
        var uow = new UnitOfWork(context);
        var currentUser = new FakeCurrentUserService();

        var parent = Category.Create("Dental", "طب الأسنان", null, null, null, "admin");
        context.Categories.Add(parent);

        var cat = Category.Create("Orthodontic", "تقويم الأسنان", "Old description", null, parent.Id, "admin");
        context.Categories.Add(cat);
        await context.SaveChangesAsync();

        var handler = new UpdateCategoryCommandHandler(uow, currentUser);
        var command = new UpdateCategoryCommand
        {
            Id = cat.Id,
            NameEn = "Orthodontic",
            NameAr = "تقويم الأسنان",
            Description = "Updated description",
            ParentCategoryId = parent.Id
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Updated description", result.Data.Description);
    }

    [Fact]
    public async Task UpdateCategory_RenamingToExistingSiblingName_ReturnsBadRequest()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);
        var uow = new UnitOfWork(context);
        var currentUser = new FakeCurrentUserService();

        var parent = Category.Create("Dental", "طب الأسنان", null, null, null, "admin");
        context.Categories.Add(parent);

        var cat1 = Category.Create("Orthodontic", "تقويم الأسنان", null, null, parent.Id, "admin");
        var cat2 = Category.Create("Endodontic", "علاج الجذور", null, null, parent.Id, "admin");
        context.Categories.AddRange(cat1, cat2);
        await context.SaveChangesAsync();

        var handler = new UpdateCategoryCommandHandler(uow, currentUser);
        var command = new UpdateCategoryCommand
        {
            Id = cat2.Id,
            NameEn = "Orthodontic",
            NameAr = "اسم عربي مختلف",
            ParentCategoryId = parent.Id
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(LocalizationKeys.Category.AlreadyExists, result.Message);
    }
}

