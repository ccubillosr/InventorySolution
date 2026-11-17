using InventoryBackend.Data;
using InventoryBackend.DTOs;
using InventoryBackend.Exceptions;
using InventoryBackend.Models;
using InventoryBackend.Repositories;
using InventoryBackend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace InventoryBackend.Tests;

public class StockServiceTests
{
    private static InventoryDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new InventoryDbContext(options);
    }

    private static StockService CreateService(InventoryDbContext context)
    {
        var stockRepository = new StockRepository(context);
        var productRepository = new ProductRepository(context);
        return new StockService(stockRepository, productRepository, NullLogger<StockService>.Instance);
    }

    private static async Task<Product> CreateProductInContextAsync(InventoryDbContext context)
    {
        var product = new Product { Name = "Test Product", Sku = "TEST-SKU", Quantity = 100, UnitPrice = 50m };
        context.Products.Add(product);
        await context.SaveChangesAsync();
        return product;
    }

    [Fact]
    public async Task CreateAsync_AddsStock_WhenProductExists()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var product = await CreateProductInContextAsync(context);
        var dto = new StockCreateDto
        {
            ProductId = product.Id,
            WarehouseLocation = "A1",
            AvailableQuantity = 50,
            ReservedQuantity = 10,
            MinimumThreshold = 5
        };

        var result = await service.CreateAsync(dto);

        Assert.True(result.Id > 0);
        Assert.Equal(product.Id, result.ProductId);
        Assert.Equal("A1", result.WarehouseLocation);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenProductNotExists()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var dto = new StockCreateDto
        {
            ProductId = 999,
            WarehouseLocation = "A1",
            AvailableQuantity = 50
        };

        await Assert.ThrowsAsync<ConflictException>(async () => await service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenStockAlreadyExistsForLocationAndProduct()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var product = await CreateProductInContextAsync(context);
        var dto = new StockCreateDto
        {
            ProductId = product.Id,
            WarehouseLocation = "B2",
            AvailableQuantity = 30
        };

        await service.CreateAsync(dto);
        var duplicate = new StockCreateDto
        {
            ProductId = product.Id,
            WarehouseLocation = "B2",
            AvailableQuantity = 20
        };

        await Assert.ThrowsAsync<ConflictException>(async () => await service.CreateAsync(duplicate));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsStocksSortedByLocation()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var product = await CreateProductInContextAsync(context);
        await service.CreateAsync(new StockCreateDto { ProductId = product.Id, WarehouseLocation = "Z", AvailableQuantity = 10 });
        await service.CreateAsync(new StockCreateDto { ProductId = product.Id, WarehouseLocation = "A", AvailableQuantity = 20 });

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("A", result[0].WarehouseLocation);
        Assert.Equal("Z", result[1].WarehouseLocation);
    }

    [Fact]
    public async Task GetByProductIdAsync_ReturnsStocksForProduct()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var product1 = await CreateProductInContextAsync(context);
        var product2 = new Product { Name = "Product 2", Sku = "SKU-2", Quantity = 50, UnitPrice = 25m };
        context.Products.Add(product2);
        await context.SaveChangesAsync();

        await service.CreateAsync(new StockCreateDto { ProductId = product1.Id, WarehouseLocation = "A", AvailableQuantity = 10 });
        await service.CreateAsync(new StockCreateDto { ProductId = product2.Id, WarehouseLocation = "B", AvailableQuantity = 20 });

        var result = await service.GetByProductIdAsync(product1.Id);

        Assert.Single(result);
        Assert.Equal(product1.Id, result[0].ProductId);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesStock_WhenExists()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var product = await CreateProductInContextAsync(context);
        var created = await service.CreateAsync(new StockCreateDto
        {
            ProductId = product.Id,
            WarehouseLocation = "C1",
            AvailableQuantity = 50
        });

        var updateDto = new StockUpdateDto { AvailableQuantity = 75, Notes = "Updated" };
        var updated = await service.UpdateAsync(created.Id, updateDto);

        Assert.True(updated);
        var result = await service.GetByIdAsync(created.Id);
        Assert.Equal(75, result?.AvailableQuantity);
        Assert.Equal("Updated", result?.Notes);
    }

    [Fact]
    public async Task DeleteAsync_DeletesStock_WhenExists()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var product = await CreateProductInContextAsync(context);
        var created = await service.CreateAsync(new StockCreateDto
        {
            ProductId = product.Id,
            WarehouseLocation = "D1",
            AvailableQuantity = 100
        });

        var deleted = await service.DeleteAsync(created.Id);

        Assert.True(deleted);
        Assert.Equal(0, await context.Stocks.CountAsync());
    }

    [Fact]
    public async Task AuditStockAsync_UpdatesLastAuditedAt()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var product = await CreateProductInContextAsync(context);
        var created = await service.CreateAsync(new StockCreateDto
        {
            ProductId = product.Id,
            WarehouseLocation = "E1",
            AvailableQuantity = 50
        });

        var audited = await service.AuditStockAsync(created.Id);

        Assert.True(audited);
        var result = await service.GetByIdAsync(created.Id);
        Assert.NotNull(result?.LastAuditedAtUtc);
    }
}
