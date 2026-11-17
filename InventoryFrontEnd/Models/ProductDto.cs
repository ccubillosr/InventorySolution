using System.ComponentModel.DataAnnotations;

namespace InventoryFrontEnd.Models;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}

public class StockDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string WarehouseLocation { get; set; } = string.Empty;
    public int AvailableQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int MinimumThreshold { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public DateTime? LastAuditedAtUtc { get; set; }
}

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int? CategoryId { get; set; }
    public CategoryDto? Category { get; set; }
    public IReadOnlyList<StockDto> Stocks { get; set; } = new List<StockDto>();
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}

public class ProductEditModel
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "El SKU es obligatorio.")]
    [StringLength(60, MinimumLength = 2)]
    [RegularExpression("^[a-zA-Z0-9-_]+$", ErrorMessage = "El SKU solo puede contener letras, números, guiones y guiones bajos.")]
    public string Sku { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "La cantidad no puede ser negativa.")]
    public int Quantity { get; set; }

    [Range(0.0, 1000000.0, ErrorMessage = "El precio debe estar entre 0 y 1,000,000.")]
    public decimal UnitPrice { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El ID de categoría debe ser válido.")]
    public int? CategoryId { get; set; }
}

public class CategoryEditModel
{
    [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres.")]
    public string? Description { get; set; }
}

public class StockEditModel
{
    [Required(ErrorMessage = "El ID del producto es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El ID del producto debe ser válido.")]
    public int ProductId { get; set; }

    [Required(ErrorMessage = "La ubicación del almacén es obligatoria.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "La ubicación debe tener entre 2 y 50 caracteres.")]
    public string WarehouseLocation { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "La cantidad disponible no puede ser negativa.")]
    public int AvailableQuantity { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "La cantidad reservada no puede ser negativa.")]
    public int ReservedQuantity { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El umbral mínimo no puede ser negativo.")]
    public int MinimumThreshold { get; set; } = 10;

    [StringLength(500, ErrorMessage = "Las notas no pueden exceder 500 caracteres.")]
    public string? Notes { get; set; }
}

