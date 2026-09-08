namespace OrderFlow.Contracts.DTOs;

public record OrderLineItemDto(string Sku, int Quantity, decimal UnitPrice);
