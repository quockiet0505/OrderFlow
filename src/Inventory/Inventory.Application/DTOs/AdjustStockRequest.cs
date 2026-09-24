using System.ComponentModel.DataAnnotations;

namespace Inventory.Application.DTOs;

public record AdjustStockRequest
{
    [Required(ErrorMessage = "Quantity is required.")]
    public int Quantity { get; init; }
}
