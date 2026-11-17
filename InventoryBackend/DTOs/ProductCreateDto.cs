using System.ComponentModel.DataAnnotations;

namespace InventoryBackend.DTOs;

public class ProductCreateDto
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
