using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using InventoryBlazorUI.Models;

namespace InventoryBlazorUI.Services
{
    public class ProductApiClient
    {
        private readonly HttpClient _httpClient;

        public ProductApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ProductDto[]> GetProductsAsync()
        {
            return await _httpClient.GetFromJsonAsync<ProductDto[]>("api/products");
        }

        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<ProductDto>($"api/products/{id}");
        }

        public async Task CreateProductAsync(ProductCreateDto productCreateDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/products", productCreateDto);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateProductAsync(int id, ProductDto productDto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/products/{id}", productDto);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteProductAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/products/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}