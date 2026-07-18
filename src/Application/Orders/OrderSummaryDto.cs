namespace EventsManager.Application.Orders;

/// <summary>Résumé d'une commande pour la liste par évènement : id, nom du client, total.</summary>
public sealed record OrderSummaryDto(Guid Id, string CustomerName, decimal Total);
