using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace InventoryApi.Tests
{
    public class ProductsControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ProductsControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Configure in-memory database or mock services if needed
                });
            }).CreateClient();
        }

        [Fact]
        public async Task GetProducts_ReturnsOk()
        {
            var response = await _client.GetAsync("/api/products");
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetProductById_ReturnsOk_WhenProductExists()
        {
            var response = await _client.GetAsync("/api/products/1");
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CreateProduct_ReturnsCreated()
        {
            var newProduct = new { Name = "Test Product", Description = "Test Description", Price = 10.0, Stock = 100 };
            var response = await _client.PostAsJsonAsync("/api/products", newProduct);
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task UpdateProduct_ReturnsNoContent_WhenProductExists()
        {
            var updatedProduct = new { Name = "Updated Product", Description = "Updated Description", Price = 15.0, Stock = 150 };
            var response = await _client.PutAsJsonAsync("/api/products/1", updatedProduct);
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteProduct_ReturnsNoContent_WhenProductExists()
        {
            var response = await _client.DeleteAsync("/api/products/1");
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}