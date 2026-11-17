using InventoryBackend.DTOs;
using InventoryBackend.Exceptions;
using InventoryBackend.Extensions;
using InventoryBackend.Models;
using InventoryBackend.Repositories;
using Microsoft.Extensions.Logging;

namespace InventoryBackend.Services;

/// <summary>
/// Servicio de dominio para operaciones CRUD de inventario (stock).
/// Maneja validaciones de negocio, auditoria y orquestación de acceso a datos.
/// </summary>
public class StockService : IStockService
{
    private readonly IStockRepository _repository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<StockService> _logger;

    public StockService(IStockRepository repository, IProductRepository productRepository, ILogger<StockService> logger)
    {
        _repository = repository;
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<StockDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var stocks = await _repository.GetAllAsync(cancellationToken);
        return stocks.Select(ToDto).AsReadOnlyList();
    }

    public async Task<StockDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var stock = await _repository.GetByIdAsync(id, cancellationToken);
        return stock.MapToDto(ToDto);
    }

    public async Task<IReadOnlyList<StockDto>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        var stocks = await _repository.GetByProductIdAsync(productId, cancellationToken);
        return stocks.Select(ToDto).AsReadOnlyList();
    }

    public async Task<StockDto> CreateAsync(StockCreateDto dto, CancellationToken cancellationToken = default)
    {
        // Validar que el producto existe
        if (!await _productRepository.ExistsAsync(dto.ProductId, cancellationToken))
        {
            throw new ConflictException($"El producto con ID {dto.ProductId} no existe.");
        }

        // Validar ubicación única para el producto
        var existingStock = await _repository.GetByProductAndLocationAsync(dto.ProductId, dto.WarehouseLocation, cancellationToken);
        if (existingStock is not null)
        {
            throw new ConflictException($"Ya existe stock para el producto {dto.ProductId} en la ubicación '{dto.WarehouseLocation}'.");
        }

        var stock = new Stock
        {
            ProductId = dto.ProductId,
            WarehouseLocation = dto.WarehouseLocation.SafeTrim()!,
            AvailableQuantity = dto.AvailableQuantity,
            ReservedQuantity = dto.ReservedQuantity,
            MinimumThreshold = dto.MinimumThreshold,
            Notes = dto.Notes.SafeTrim(),
            CreatedAtUtc = DateTime.UtcNow,
            LastAuditedAtUtc = DateTime.UtcNow
        };

        await _repository.AddAsync(stock, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Stock creado: Id={StockId}, ProductId={ProductId}, Location={Location}",
            stock.Id, dto.ProductId, stock.WarehouseLocation);
        return ToDto(stock);
    }

    public async Task<bool> UpdateAsync(int id, StockUpdateDto dto, CancellationToken cancellationToken = default)
    {
        // Usar GetByIdTrackedAsync para que EF Core pueda rastrear cambios
        var stock = await _repository.GetByIdTrackedAsync(id, cancellationToken);
        if (stock is null)
        {
            return false;
        }

        // Solo aplicar cambios si se proporcionaron
        if (dto.AvailableQuantity.HasValue)
            stock.AvailableQuantity = dto.AvailableQuantity.Value;

        if (dto.ReservedQuantity.HasValue)
            stock.ReservedQuantity = dto.ReservedQuantity.Value;

        if (dto.MinimumThreshold.HasValue)
            stock.MinimumThreshold = dto.MinimumThreshold.Value;

        if (dto.Notes is not null)
            stock.Notes = dto.Notes.SafeTrim();

        stock.UpdatedAtUtc = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Stock actualizado: Id={StockId}", stock.Id);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        // Usar GetByIdTrackedAsync para que EF Core rastreee y luego pueda eliminar
        var stock = await _repository.GetByIdTrackedAsync(id, cancellationToken);
        if (stock is null)
        {
            return false;
        }

        _repository.Remove(stock);
        await _repository.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Stock eliminado: Id={StockId}", id);
        return true;
    }

    public async Task<bool> AuditStockAsync(int id, CancellationToken cancellationToken = default)
    {
        var stock = await _repository.GetByIdAsync(id, cancellationToken);
        if (stock is null)
        {
            return false;
        }

        stock.LastAuditedAtUtc = DateTime.UtcNow;
        await _repository.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Stock auditado: Id={StockId}", id);
        return true;
    }

    /// <summary>Convierte una entidad Stock a StockDto.</summary>
    private static StockDto ToDto(Stock stock) => new()
    {
        Id = stock.Id,
        ProductId = stock.ProductId,
        WarehouseLocation = stock.WarehouseLocation,
        AvailableQuantity = stock.AvailableQuantity,
        ReservedQuantity = stock.ReservedQuantity,
        MinimumThreshold = stock.MinimumThreshold,
        Notes = stock.Notes,
        CreatedAtUtc = stock.CreatedAtUtc,
        UpdatedAtUtc = stock.UpdatedAtUtc,
        LastAuditedAtUtc = stock.LastAuditedAtUtc
    };
}
