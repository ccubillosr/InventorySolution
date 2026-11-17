using System.ComponentModel.DataAnnotations;

namespace InventoryBackend.DTOs;

public class StockUpdateDto
{
    [Range(0, int.MaxValue, ErrorMessage = "La cantidad disponible no puede ser negativa.")]
    public int? AvailableQuantity { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "La cantidad reservada no puede ser negativa.")]
    public int? ReservedQuantity { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El umbral mínimo no puede ser negativo.")]
    public int? MinimumThreshold { get; set; }

    [StringLength(500, ErrorMessage = "Las notas no pueden exceder 500 caracteres.")]
    public string? Notes { get; set; }
}
