using System.ComponentModel.DataAnnotations;

namespace InventoryBackend.Models;

/// <summary>
/// Control de inventario de productos por almacén. Seguridad: se validan cantidades y auditoría.
/// </summary>
public class Stock
{
    public int Id { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    [MaxLength(50)]
    public string WarehouseLocation { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int AvailableQuantity { get; set; }

    [Range(0, int.MaxValue)]
    public int ReservedQuantity { get; set; }

    [Range(0, int.MaxValue)]
    public int MinimumThreshold { get; set; } = 10;

    [MaxLength(500)]
    public string? Notes { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public DateTime? LastAuditedAtUtc { get; set; }

    public Product? Product { get; set; }
}
