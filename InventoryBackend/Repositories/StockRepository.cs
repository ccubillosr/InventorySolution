using InventoryBackend.Data;
using InventoryBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryBackend.Repositories;

public class StockRepository : IStockRepository
{
    private readonly InventoryDbContext _context;

    public StockRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Stock>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Stocks
            .AsNoTracking()
            .Include(s => s.Product)
            .OrderBy(s => s.WarehouseLocation)
            .ToListAsync(cancellationToken);
    }

    public async Task<Stock?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Stocks
            .AsNoTracking()
            .Include(s => s.Product)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    /// <summary>
    /// Obtiene un stock tracked (para operaciones de actualización).
    /// Úsalo en Update/Delete operations que necesiten cambiar tracking.
    /// </summary>
    public async Task<Stock?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Stocks
            .Include(s => s.Product)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Stock>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        return await _context.Stocks
            .AsNoTracking()
            .Where(s => s.ProductId == productId)
            .OrderBy(s => s.WarehouseLocation)
            .ToListAsync(cancellationToken);
    }

    public async Task<Stock?> GetByProductAndLocationAsync(int productId, string warehouseLocation, CancellationToken cancellationToken = default)
    {
        return await _context.Stocks
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseLocation == warehouseLocation, cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Stocks
            .AsNoTracking()
            .AnyAsync(s => s.Id == id, cancellationToken);
    }

    public async Task AddAsync(Stock stock, CancellationToken cancellationToken = default)
    {
        await _context.Stocks.AddAsync(stock, cancellationToken);
    }

    public void Remove(Stock stock)
    {
        _context.Stocks.Remove(stock);
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken) >= 0;
    }
}
