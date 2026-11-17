using InventoryBackend.DTOs;
using InventoryBackend.Exceptions;
using InventoryBackend.Extensions;
using InventoryBackend.Models;
using InventoryBackend.Repositories;
using Microsoft.Extensions.Logging;

namespace InventoryBackend.Services;

/// <summary>
/// Servicio de dominio para operaciones CRUD de productos.
/// Maneja validaciones de negocio y orquestación de acceso a datos.
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProductRepository repository, ICategoryRepository categoryRepository, ILogger<ProductService> logger)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await _repository.GetAllAsync(cancellationToken);
        return products.Select(ToDto).AsReadOnlyList();
    }

    public async Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(id, cancellationToken);
        return product.MapToDto(ToDto);
    }

    public async Task<ProductDto> CreateAsync(ProductCreateDto dto, CancellationToken cancellationToken = default)
    {
        // Validar SKU único
        if (await _repository.SkuExistsAsync(dto.Sku, cancellationToken: cancellationToken))
        {
            throw new ConflictException($"Ya existe un producto con el SKU '{dto.Sku}'.");
        }

        // Validar categoría si se proporciona
        if (dto.CategoryId.HasValue && !await _categoryRepository.ExistsAsync(dto.CategoryId.Value, cancellationToken))
        {
            throw new ConflictException($"La categoría con ID {dto.CategoryId} no existe.");
        }

        var product = new Product
        {
            Name = dto.Name.SafeTrim()!,
            Description = dto.Description.SafeTrim(),
            Sku = dto.Sku.SafeTrim()!,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice,
            CategoryId = dto.CategoryId,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _repository.AddAsync(product, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Producto creado: Id={ProductId}, Sku={Sku}", product.Id, product.Sku);
        return ToDto(product);
    }

    public async Task<bool> UpdateAsync(int id, ProductUpdateDto dto, CancellationToken cancellationToken = default)
    {
        // Usar GetByIdTrackedAsync para que EF Core pueda rastrear cambios
        var product = await _repository.GetByIdTrackedAsync(id, cancellationToken);
        if (product is null)
        {
            return false;
        }

        // Validar SKU único solo si cambió
        if (dto.Sku != product.Sku && await _repository.SkuExistsAsync(dto.Sku, cancellationToken: cancellationToken))
        {
            throw new ConflictException($"Ya existe un producto con el SKU '{dto.Sku}'.");
        }

        // Validar categoría si cambió y se proporciona
        if (dto.CategoryId != product.CategoryId && dto.CategoryId.HasValue && 
            !await _categoryRepository.ExistsAsync(dto.CategoryId.Value, cancellationToken))
        {
            throw new ConflictException($"La categoría con ID {dto.CategoryId} no existe.");
        }

        // Aplicar cambios
        product.Name = dto.Name.SafeTrim()!;
        product.Description = dto.Description.SafeTrim();
        product.Sku = dto.Sku.SafeTrim()!;
        product.Quantity = dto.Quantity;
        product.UnitPrice = dto.UnitPrice;
        product.CategoryId = dto.CategoryId;
        product.UpdatedAtUtc = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Producto actualizado: Id={ProductId}, Sku={Sku}", product.Id, product.Sku);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        // Usar GetByIdTrackedAsync para que EF Core rastreee y luego pueda eliminar
        var product = await _repository.GetByIdTrackedAsync(id, cancellationToken);
        if (product is null)
        {
            return false;
        }

        _repository.Remove(product);
        await _repository.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Producto eliminado: Id={ProductId}", id);
        return true;
    }

    /// <summary>
    /// Convierte una entidad Product a ProductDto.
    /// Nota: Requiere que el product tenga Category y Stocks ya cargados (o serán null/vacío).
    /// </summary>
    private static ProductDto ToDto(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Description = product.Description,
        Sku = product.Sku,
        Quantity = product.Quantity,
        UnitPrice = product.UnitPrice,
        CategoryId = product.CategoryId,
        Category = product.Category is null ? null : new CategoryDto
        {
            Id = product.Category.Id,
            Name = product.Category.Name,
            Description = product.Category.Description,
            CreatedAtUtc = product.Category.CreatedAtUtc,
            UpdatedAtUtc = product.Category.UpdatedAtUtc
        },
        Stocks = product.Stocks?.Select(s => new StockDto
        {
            Id = s.Id,
            ProductId = s.ProductId,
            WarehouseLocation = s.WarehouseLocation,
            AvailableQuantity = s.AvailableQuantity,
            ReservedQuantity = s.ReservedQuantity,
            MinimumThreshold = s.MinimumThreshold,
            Notes = s.Notes,
            CreatedAtUtc = s.CreatedAtUtc,
            UpdatedAtUtc = s.UpdatedAtUtc,
            LastAuditedAtUtc = s.LastAuditedAtUtc
        }).ToList() ?? new List<StockDto>(),
        CreatedAtUtc = product.CreatedAtUtc,
        UpdatedAtUtc = product.UpdatedAtUtc
    };
}

