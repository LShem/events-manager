using EventsManager.Domain.Orders;

namespace EventsManager.Application.Orders;

public static class OrderMappings
{
    public static OrderDto ToDto(this Order order)
    {
        return new OrderDto(
            order.Id.Value,
            order.EventId.Value,
            order.CustomerName,
            [.. order.Lines.Select(line => line.ToDto())],
            order.Total.Amount);
    }

    public static OrderLineDto ToDto(this OrderLine line)
    {
        return new OrderLineDto(line.Label, line.UnitPrice.Amount, line.Quantity, line.Subtotal.Amount);
    }

    public static OrderSummaryDto ToSummaryDto(this Order order)
    {
        return new OrderSummaryDto(order.Id.Value, order.CustomerName, order.Total.Amount);
    }
}
