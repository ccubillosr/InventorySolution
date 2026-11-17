using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using InventoryFrontEnd.Models;

namespace InventoryFrontEnd.Services;

public class ProductApiService : IProductApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductApiService> _logger;

    public ProductApiService(HttpClient httpClient, ILogger<ProductApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<ProductDto>> GetAllAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<ProductDto>>("api/products");
        return result ?? new List<ProductDto>();
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"api/products/{id}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProductDto>();
    }

    public async Task<(bool Success, string? Error, ProductDto? Product)> CreateAsync(ProductEditModel model)
    {
        var response = await _httpClient.PostAsJsonAsync("api/products", model);
        if (response.IsSuccessStatusCode)
        {
            var created = await response.Content.ReadFromJsonAsync<ProductDto>();
            return (true, null, created);
        }

        var error = await ExtractErrorAsync(response);
        return (false, error, null);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(int id, ProductEditModel model)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/products/{id}", model);
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        var error = await ExtractErrorAsync(response);
        return (false, error);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/products/{id}");
        return response.IsSuccessStatusCode;
    }

    private async Task<string> ExtractErrorAsync(HttpResponseMessage response)
    {
        try
        {
            using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);
            if (document.RootElement.TryGetProperty("message", out var messageProp))
            {
                return messageProp.GetString() ?? "Ocurri\u00f3 un error al procesar la solicitud.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo interpretar el cuerpo del error de la API.");
        }

        return "Ocurri\u00f3 un error al procesar la solicitud.";
    }
}
