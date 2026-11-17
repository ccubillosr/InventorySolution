using System.ComponentModel.DataAnnotations;

namespace InventoryBackend.DTOs;

public class CategoryUpdateDto
{
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    public string? Name { get; set; }

    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres.")]
    public string? Description { get; set; }
}
