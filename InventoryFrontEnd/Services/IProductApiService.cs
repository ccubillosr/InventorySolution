using InventoryFrontEnd.Models;

namespace InventoryFrontEnd.Services;

public interface IProductApiService
{
    Task<List<ProductDto>> GetAllAsync();
    Task<ProductDto?> GetByIdAsync(int id);
    Task<(bool Success, string? Error, ProductDto? Product)> CreateAsync(ProductEditModel model);
    Task<(bool Success, string? Error)> UpdateAsync(int id, ProductEditModel model);
    Task<bool> DeleteAsync(int id);
}
