using InventoryBackend.Data;
using InventoryBackend.DTOs;
using InventoryBackend.Exceptions;
using InventoryBackend.Repositories;
using InventoryBackend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace InventoryBackend.Tests;

public class CategoryServiceTests
{
    private static InventoryDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new InventoryDbContext(options);
    }

    private static CategoryService CreateService(InventoryDbContext context)
    {
        var repository = new CategoryRepository(context);
        return new CategoryService(repository, NullLogger<CategoryService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_AddsCategory_WhenNameIsUnique()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var dto = new CategoryCreateDto { Name = "Electronics", Description = "Electronic devices" };

        var result = await service.CreateAsync(dto);

        Assert.True(result.Id > 0);
        Assert.Equal("Electronics", result.Name);
        Assert.Equal(1, await context.Categories.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenNameAlreadyExists()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var dto = new CategoryCreateDto { Name = "Phones", Description = "Mobile phones" };
        await service.CreateAsync(dto);

        var duplicate = new CategoryCreateDto { Name = "Phones", Description = "Duplicate" };

        await Assert.ThrowsAsync<ConflictException>(async () => await service.CreateAsync(duplicate));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsCategoriesSortedByName()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        await service.CreateAsync(new CategoryCreateDto { Name = "Zebras", Description = "Z" });
        await service.CreateAsync(new CategoryCreateDto { Name = "Apples", Description = "A" });

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Apples", result[0].Name);
        Assert.Equal("Zebras", result[1].Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCategory_WhenExists()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var created = await service.CreateAsync(new CategoryCreateDto { Name = "Test" });

        var result = await service.GetByIdAsync(created.Id);

        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var result = await service.GetByIdAsync(99);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesCategory_WhenExists()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var created = await service.CreateAsync(new CategoryCreateDto { Name = "Old Name" });
        var updateDto = new CategoryUpdateDto { Name = "New Name", Description = "Updated description" };

        var updated = await service.UpdateAsync(created.Id, updateDto);

        Assert.True(updated);
        var result = await service.GetByIdAsync(created.Id);
        Assert.Equal("New Name", result?.Name);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenNotExists()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var result = await service.UpdateAsync(99, new CategoryUpdateDto { Name = "Test" });

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_DeletesCategory_WhenExists()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var created = await service.CreateAsync(new CategoryCreateDto { Name = "ToDelete" });

        var deleted = await service.DeleteAsync(created.Id);

        Assert.True(deleted);
        Assert.Equal(0, await context.Categories.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNotExists()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var result = await service.DeleteAsync(99);

        Assert.False(result);
    }
}
