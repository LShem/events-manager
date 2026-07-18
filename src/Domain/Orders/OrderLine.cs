namespace EventsManager.Domain.Orders;

/// <summary>
/// Ligne d'une commande : snapshot immuable d'un libellé et d'un prix saisis
/// librement, figés à la création — aucune référence à un catalogue.
/// Invariants et préconditions garantis ici, indépendamment de toute validation
/// amont : libellé non vide (ni blanc) et limité à <see cref="LabelMaxLength"/>
/// caractères après trim ; prix unitaire obligatoire (un <see cref="Money"/>
/// construit, jamais <c>default</c>) ; quantité entre <see cref="QuantityMin"/>
/// et <see cref="QuantityMax"/>. Le sous-total est calculé, jamais stocké.
/// </summary>
public sealed class OrderLine
{
    public const int LabelMaxLength = 100;
    public const int QuantityMin = 1;
    public const int QuantityMax = 20;

    public string Label { get; }

    public Money UnitPrice { get; }

    public int Quantity { get; }

    public Money Subtotal => UnitPrice * Quantity;

    private OrderLine(string label, Money unitPrice, int quantity)
    {
        Label = label;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public static OrderLine Create(string label, Money unitPrice, int quantity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);

        var trimmedLabel = label.Trim();

        if (trimmedLabel.Length > LabelMaxLength)
        {
            throw new ArgumentException(
                $"Le libellé d'une ligne ne peut pas dépasser {LabelMaxLength} caractères (reçu : {trimmedLabel.Length}).",
                nameof(label));
        }

        if (unitPrice == default)
        {
            throw new ArgumentException(
                "Le prix unitaire d'une ligne est obligatoire.",
                nameof(unitPrice));
        }

        if (quantity is < QuantityMin or > QuantityMax)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                $"La quantité d'une ligne doit être comprise entre {QuantityMin} et {QuantityMax} (reçue : {quantity}).");
        }

        return new OrderLine(trimmedLabel, unitPrice, quantity);
    }
}
