namespace EventsManager.Domain.Orders;

/// <summary>
/// Montant en euros — la devise est implicite et n'est pas représentée dans le modèle.
/// Invariants garantis ici : montant strictement positif (une gratuité ne s'encode
/// pas dans une commande) et deux décimales maximum. L'addition et la multiplication
/// par un entier strictement positif sont exactes et préservent ces invariants ;
/// aucune division dans le modèle, donc aucune règle d'arrondi.
/// </summary>
public readonly record struct Money
{
    public const int MaxDecimalPlaces = 2;

    public decimal Amount { get; }

    private Money(decimal amount)
    {
        Amount = amount;
    }

    public static Money From(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                $"Un montant doit être strictement positif (reçu : {amount}).");
        }

        if (decimal.Round(amount, MaxDecimalPlaces) != amount)
        {
            throw new ArgumentException(
                $"Un montant ne peut pas porter plus de deux décimales (reçu : {amount}).",
                nameof(amount));
        }

        return new Money(amount);
    }

    public static Money operator +(Money left, Money right)
    {
        return new Money(left.Amount + right.Amount);
    }

    public static Money operator *(Money money, int factor)
    {
        if (factor <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(factor),
                $"Le facteur de multiplication d'un montant doit être strictement positif (reçu : {factor}).");
        }

        return new Money(money.Amount * factor);
    }
}
