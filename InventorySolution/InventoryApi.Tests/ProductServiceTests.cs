using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using InventoryApi.Models;
using InventoryApi.Services;

namespace InventoryApi.Tests
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _mockRepository;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _mockRepository = new Mock<IProductRepository>();
            _productService = new ProductService(_mockRepository.Object);
        }

        [Fact]
        public async Task GetAllProducts_ReturnsAllProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Product 1", Description = "Description 1", Price = 10.0m, Stock = 100 },
                new Product { Id = 2, Name = "Product 2", Description = "Description 2", Price = 20.0m, Stock = 200 }
            };
            _mockRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(products);

            // Act
            var result = await _productService.GetAllProductsAsync();

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetProductById_ReturnsProduct()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Product 1", Description = "Description 1", Price = 10.0m, Stock = 100 };
            _mockRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(product);

            // Act
            var result = await _productService.GetProductByIdAsync(1);

            // Assert
            Assert.Equal("Product 1", result.Name);
        }

        [Fact]
        public async Task CreateProduct_AddsProduct()
        {
            // Arrange
            var newProduct = new Product { Name = "New Product", Description = "New Description", Price = 15.0m, Stock = 150 };
            _mockRepository.Setup(repo => repo.AddAsync(newProduct)).ReturnsAsync(newProduct);

            // Act
            var result = await _productService.CreateProductAsync(newProduct);

            // Assert
            Assert.Equal("New Product", result.Name);
        }

        [Fact]
        public async Task UpdateProduct_UpdatesProduct()
        {
            // Arrange
            var existingProduct = new Product { Id = 1, Name = "Product 1", Description = "Description 1", Price = 10.0m, Stock = 100 };
            _mockRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(existingProduct);
            var updatedProduct = new Product { Id = 1, Name = "Updated Product", Description = "Updated Description", Price = 12.0m, Stock = 120 };

            // Act
            await _productService.UpdateProductAsync(updatedProduct);

            // Assert
            _mockRepository.Verify(repo => repo.UpdateAsync(updatedProduct), Times.Once);
        }

        [Fact]
        public async Task DeleteProduct_RemovesProduct()
        {
            // Arrange
            var productId = 1;
            _mockRepository.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync(new Product { Id = productId });

            // Act
            await _productService.DeleteProductAsync(productId);

            // Assert
            _mockRepository.Verify(repo => repo.DeleteAsync(productId), Times.Once);
        }
    }
}