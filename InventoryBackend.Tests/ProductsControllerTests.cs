using InventoryBackend.Controllers;
using InventoryBackend.DTOs;
using InventoryBackend.Exceptions;
using InventoryBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace InventoryBackend.Tests;

public class ProductsControllerTests
{
    private readonly Mock<IProductService> _serviceMock = new();
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _controller = new ProductsController(_serviceMock.Object, NullLogger<ProductsController>.Instance);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithProducts()
    {
        var products = new List<ProductDto> { new() { Id = 1, Name = "Laptop", Sku = "SKU-1" } };
        _serviceMock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(products);

        var result = await _controller.GetAll(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(products, okResult.Value);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenProductMissing()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((ProductDto?)null);

        var result = await _controller.GetById(1, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenProductExists()
    {
        var product = new ProductDto { Id = 1, Name = "Laptop", Sku = "SKU-1" };
        _serviceMock.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var result = await _controller.GetById(1, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(product, okResult.Value);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WhenSuccessful()
    {
        var dto = new ProductCreateDto { Name = "Laptop", Sku = "SKU-1", Quantity = 1, UnitPrice = 10 };
        var created = new ProductDto { Id = 1, Name = "Laptop", Sku = "SKU-1" };
        _serviceMock.Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>())).ReturnsAsync(created);

        var result = await _controller.Create(dto, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(created, createdResult.Value);
    }

    [Fact]
    public async Task Create_ReturnsConflict_WhenSkuDuplicated()
    {
        var dto = new ProductCreateDto { Name = "Laptop", Sku = "SKU-1", Quantity = 1, UnitPrice = 10 };
        _serviceMock.Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException("duplicado"));

        var result = await _controller.Create(dto, CancellationToken.None);

        Assert.IsType<ConflictObjectResult>(result.Result);
    }

    [Fact]
    public async Task Update_ReturnsNoContent_WhenSuccessful()
    {
        var dto = new ProductUpdateDto { Name = "Laptop", Sku = "SKU-1", Quantity = 1, UnitPrice = 10 };
        _serviceMock.Setup(s => s.UpdateAsync(1, dto, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _controller.Update(1, dto, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenProductMissing()
    {
        var dto = new ProductUpdateDto { Name = "Laptop", Sku = "SKU-1", Quantity = 1, UnitPrice = 10 };
        _serviceMock.Setup(s => s.UpdateAsync(1, dto, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _controller.Update(1, dto, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenSuccessful()
    {
        _serviceMock.Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _controller.Delete(1, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenProductMissing()
    {
        _serviceMock.Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _controller.Delete(1, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }
}
