using InventoryBackend.Models;

namespace InventoryBackend.Repositories;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>Obtiene producto tracked para modificación. Úsalo en Update/Delete operations.</summary>
    Task<Product?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> SkuExistsAsync(string sku, int? excludeId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    void Remove(Product product);
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
}
