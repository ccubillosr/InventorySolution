using InventoryBackend.Models;

namespace InventoryBackend.Repositories;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>Obtiene categoría con sus productos incluidos. Úsalo cuando necesites acceder a Products.</summary>
    Task<Category?> GetByIdWithProductsAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>Obtiene categoría tracked para modificación. Úsalo en Update/Delete operations.</summary>
    Task<Category?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Category category, CancellationToken cancellationToken = default);
    void Remove(Category category);
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
}
