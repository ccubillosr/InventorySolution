namespace InventoryBackend.DTOs;

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
