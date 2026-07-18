using EventsManager.Domain.Orders;

namespace EventsManager.Application.Orders;

/// <summary>
/// Use case : lire une commande par son identité, avec ses lignes et son total.
/// Retourne null si introuvable (la traduction en 404 appartient à la couche au-dessus).
/// L'identifiant arrive en <see cref="Guid"/> brut : la conversion via
/// <see cref="OrderId.From"/> laisse le domaine intercepter les valeurs invalides.
/// </summary>
public sealed class GetOrderQueryHandler(IOrderRepository orderRepository)
{
    private readonly IOrderRepository _orderRepository = orderRepository;

    public async Task<OrderDto?> HandleAsync(GetOrderQuery query, CancellationToken cancellationToken)
    {
        var id = OrderId.From(query.Id);
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);

        return order?.ToDto();
    }
}
