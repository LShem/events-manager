namespace EventsManager.Application.Orders;

/// <summary>
/// Contrat d'entrée du use case « créer une commande » — primitifs/BCL uniquement,
/// la conversion vers les types du domaine vit dans le handler.
/// </summary>
public sealed record CreateOrderCommand(
    Guid EventId,
    string CustomerName,
    IReadOnlyList<CreateOrderCommand.Line> Lines)
{
    /// <summary>Ligne saisie librement : libellé et prix figés à la création (snapshot).</summary>
    public sealed record Line(string Label, decimal UnitPrice, int Quantity);
}
