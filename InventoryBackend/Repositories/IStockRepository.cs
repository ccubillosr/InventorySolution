using InventoryBackend.Models;

namespace InventoryBackend.Repositories;

public interface IStockRepository
{
    Task<IReadOnlyList<Stock>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Stock?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>Obtiene stock tracked para modificación. Úsalo en Update/Delete operations.</summary>
    Task<Stock?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Stock>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);
    Task<Stock?> GetByProductAndLocationAsync(int productId, string warehouseLocation, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(Stock stock, CancellationToken cancellationToken = default);
    void Remove(Stock stock);
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
}
