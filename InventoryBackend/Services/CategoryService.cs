using InventoryBackend.DTOs;
using InventoryBackend.Exceptions;
using InventoryBackend.Extensions;
using InventoryBackend.Models;
using InventoryBackend.Repositories;
using Microsoft.Extensions.Logging;

namespace InventoryBackend.Services;

/// <summary>
/// Servicio de dominio para operaciones CRUD de categorías.
/// Maneja validaciones de negocio y orquestación de acceso a datos.
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(ICategoryRepository repository, ILogger<CategoryService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _repository.GetAllAsync(cancellationToken);
        return categories.Select(ToDto).AsReadOnlyList();
    }

    public async Task<CategoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _repository.GetByIdAsync(id, cancellationToken);
        return category.MapToDto(ToDto);
    }

    public async Task<CategoryDto> CreateAsync(CategoryCreateDto dto, CancellationToken cancellationToken = default)
    {
        // Validar nombre único
        if (await _repository.NameExistsAsync(dto.Name, cancellationToken: cancellationToken))
        {
            throw new ConflictException($"Ya existe una categoría con el nombre '{dto.Name}'.");
        }

        var category = new Category
        {
            Name = dto.Name.SafeTrim()!,
            Description = dto.Description.SafeTrim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        await _repository.AddAsync(category, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Categoría creada: Id={CategoryId}, Name={Name}", category.Id, category.Name);
        return ToDto(category);
    }

    public async Task<bool> UpdateAsync(int id, CategoryUpdateDto dto, CancellationToken cancellationToken = default)
    {
        // Usar GetByIdTrackedAsync para que EF Core pueda rastrear cambios
        var category = await _repository.GetByIdTrackedAsync(id, cancellationToken);
        if (category is null)
        {
            return false;
        }

        // Validar nombre único solo si cambió
        if (!string.IsNullOrEmpty(dto.Name) && dto.Name != category.Name && 
            await _repository.NameExistsAsync(dto.Name, cancellationToken: cancellationToken))
        {
            throw new ConflictException($"Ya existe una categoría con el nombre '{dto.Name}'.");
        }

        // Aplicar cambios si se proporcionaron
        if (!string.IsNullOrEmpty(dto.Name))
            category.Name = dto.Name.SafeTrim()!;

        if (dto.Description is not null)
            category.Description = dto.Description.SafeTrim();

        category.UpdatedAtUtc = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Categoría actualizada: Id={CategoryId}", category.Id);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        // Usar GetByIdTrackedAsync para que EF Core rastreee y luego pueda eliminar
        var category = await _repository.GetByIdTrackedAsync(id, cancellationToken);
        if (category is null)
        {
            return false;
        }

        _repository.Remove(category);
        await _repository.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Categoría eliminada: Id={CategoryId}", id);
        return true;
    }

    /// <summary>Convierte una entidad Category a CategoryDto.</summary>
    private static CategoryDto ToDto(Category category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Description = category.Description,
        CreatedAtUtc = category.CreatedAtUtc,
        UpdatedAtUtc = category.UpdatedAtUtc
    };
}
