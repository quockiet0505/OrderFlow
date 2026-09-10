using System.Collections.Generic;

namespace Orders.Application.DTOs;

public record CreateOrderApiRequest(
    string CustomerId,
    List<CreateOrderLineApiRequest> Lines
);

public record CreateOrderLineApiRequest(
    string Sku,
    int Quantity,
    decimal UnitPrice
);
