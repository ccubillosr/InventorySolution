using System.ComponentModel.DataAnnotations;

namespace InventoryBackend.DTOs;

public class StockCreateDto
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
