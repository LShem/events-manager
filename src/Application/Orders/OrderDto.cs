namespace EventsManager.Application.Orders;

public sealed record OrderDto(
    Guid Id,
    Guid EventId,
    string CustomerName,
    IReadOnlyList<OrderLineDto> Lines,
    decimal Total);
