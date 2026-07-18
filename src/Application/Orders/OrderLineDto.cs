namespace EventsManager.Application.Orders;

public sealed record OrderLineDto(string Label, decimal UnitPrice, int Quantity, decimal Subtotal);
