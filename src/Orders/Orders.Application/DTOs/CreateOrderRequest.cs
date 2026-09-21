using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Orders.Application.DTOs;

public record CreateOrderApiRequest
{
    [Required(ErrorMessage = "CustomerId is required.")]
    public string CustomerId { get; init; } = string.Empty;

    [Required(ErrorMessage = "Order lines are required.")]
    [MinLength(1, ErrorMessage = "Order must contain at least one line.")]
    public List<CreateOrderLineApiRequest> Lines { get; init; } = new();
}

public record CreateOrderLineApiRequest
{
    [Required(ErrorMessage = "SKU is required.")]
    public string Sku { get; init; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
    public int Quantity { get; init; }

    [Range(0, double.MaxValue, ErrorMessage = "UnitPrice cannot be negative.")]
    public decimal UnitPrice { get; init; }
}