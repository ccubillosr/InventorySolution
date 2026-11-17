using InventoryBackend.Data;
using InventoryBackend.DTOs;
using InventoryBackend.Exceptions;
using InventoryBackend.Models;
using InventoryBackend.Repositories;
using InventoryBackend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace InventoryBackend.Tests;

public class ProductServiceTests
{
    private static InventoryDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new InventoryDbContext(options);
    }

    private static ProductService CreateService(InventoryDbContext context)
    {
        var repository = new ProductRepository(context);
        var categoryRepository = new CategoryRepository(context);
        return new ProductService(repository, categoryRepository, NullLogger<ProductService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_AddsProduct_WhenSkuIsUnique()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var dto = new ProductCreateDto { Name = "Laptop", Sku = "SKU-001", Quantity = 5, UnitPrice = 999.99m };

        var result = await service.CreateAsync(dto);

        Assert.True(result.Id > 0);
        Assert.Equal("Laptop", result.Name);
        Assert.Equal(1, await context.Products.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenSkuAlreadyExists()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var dto = new ProductCreateDto { Name = "Laptop", Sku = "SKU-DUP", Quantity = 5, UnitPrice = 999.99m };
        await service.CreateAsync(dto);

        var duplicate = new ProductCreateDto { Name = "Otro", Sku = "SKU-DUP", Quantity = 1, UnitPrice = 10m };

        await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(duplicate));
    }

    [Fact]
    public async Task CreateAsync_AddsProductWithCategory_WhenCategoryExists()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var category = new Category { Name = "Electronics" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var dto = new ProductCreateDto
        {
            Name = "Phone",
            Sku = "SKU-PHONE",
            Quantity = 10,
            UnitPrice = 599.99m,
            CategoryId = category.Id
        };

        var result = await service.CreateAsync(dto);

        Assert.True(result.Id > 0);
        Assert.Equal(category.Id, result.CategoryId);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenCategoryDoesNotExist()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var dto = new ProductCreateDto
        {
            Name = "Phone",
            Sku = "SKU-PHONE",
            Quantity = 10,
            UnitPrice = 599.99m,
            CategoryId = 999
        };

        await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllProducts()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        await service.CreateAsync(new ProductCreateDto { Name = "A", Sku = "SKU-A", Quantity = 1, UnitPrice = 1 });
        await service.CreateAsync(new ProductCreateDto { Name = "B", Sku = "SKU-B", Quantity = 2, UnitPrice = 2 });

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesProduct_WhenExists()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var created = await service.CreateAsync(new ProductCreateDto { Name = "Old", Sku = "SKU-U", Quantity = 1, UnitPrice = 1 });

        var updated = await service.UpdateAsync(created.Id, new ProductUpdateDto { Name = "New", Sku = "SKU-U", Quantity = 10, UnitPrice = 20 });

        Assert.True(updated);
        var fetched = await service.GetByIdAsync(created.Id);
        Assert.Equal("New", fetched!.Name);
        Assert.Equal(10, fetched.Quantity);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenProductDoesNotExist()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var updated = await service.UpdateAsync(999, new ProductUpdateDto { Name = "New", Sku = "SKU-X", Quantity = 1, UnitPrice = 1 });

        Assert.False(updated);
    }

    [Fact]
    public async Task DeleteAsync_RemovesProduct_WhenExists()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var created = await service.CreateAsync(new ProductCreateDto { Name = "ToDelete", Sku = "SKU-DEL", Quantity = 1, UnitPrice = 1 });

        var deleted = await service.DeleteAsync(created.Id);

        Assert.True(deleted);
        Assert.Null(await service.GetByIdAsync(created.Id));
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenProductDoesNotExist()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var deleted = await service.DeleteAsync(999);

        Assert.False(deleted);
    }
}
