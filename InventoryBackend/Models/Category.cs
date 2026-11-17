using System.ComponentModel.DataAnnotations;

namespace InventoryBackend.Models;

/// <summary>
/// Categoría de productos. Seguridad: se validan longitudes y caracteres especiales.
/// </summary>
public class Category
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
