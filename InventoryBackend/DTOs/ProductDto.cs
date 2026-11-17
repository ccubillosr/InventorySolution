namespace InventoryBackend.DTOs;

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
