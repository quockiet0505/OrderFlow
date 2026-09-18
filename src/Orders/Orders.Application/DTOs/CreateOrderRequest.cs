using System.ComponentModel.DataAnnotations;

namespace Orders.Application.DTOs;

public record CreateOrderApiRequest(
    [property: Required(ErrorMessage = "CustomerId is required.")]
    string CustomerId,

    [property: Required(ErrorMessage = "Order lines are required.")]
    [property: MinLength(1, ErrorMessage = "Order must contain at least one line.")]
    List<CreateOrderLineApiRequest> Lines
);

public record CreateOrderLineApiRequest(
    [property: Required(ErrorMessage = "SKU is required.")]
    string Sku,

    [property: Range(1, int.MaxValue,
        ErrorMessage = "Quantity must be greater than zero.")]
    int Quantity,

    [property: Range(0, double.MaxValue,
        ErrorMessage = "UnitPrice cannot be negative.")]
    decimal UnitPrice
);