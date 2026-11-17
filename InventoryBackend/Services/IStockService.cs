using InventoryBackend.DTOs;

namespace InventoryBackend.Services;

public interface IStockService
{
    Task<IReadOnlyList<StockDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<StockDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StockDto>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);
    Task<StockDto> CreateAsync(StockCreateDto dto, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, StockUpdateDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> AuditStockAsync(int id, CancellationToken cancellationToken = default);
}
