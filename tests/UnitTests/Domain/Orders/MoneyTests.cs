using EventsManager.Domain.Orders;

namespace EventsManager.UnitTests.Domain.Orders;

public class MoneyTests
{
    [Fact]
    public void Money_DoesNotRepresentCurrency()
    {
        // OR-M1 : la devise (EUR) est implicite, non représentée dans le modèle —
        // Money n'expose qu'un montant, aucune propriété de devise.
        typeof(Money).GetProperties().Should().ContainSingle().Which.Name.Should().Be(nameof(Money.Amount));
    }

    [Fact]
    public void From_WithTwoDecimals_PreservesAmount()
    {
        // OR-M1 et OR-M2 : 4.50 est valide (exemple du critère), le montant est préservé tel quel.
        var money = Money.From(4.50m);

        money.Amount.Should().Be(4.50m);
    }

    [Fact]
    public void From_WithWholeAmount_Succeeds()
    {
        // OR-M2 : deux décimales *maximum* — un montant entier est valide.
        var money = Money.From(5m);

        money.Amount.Should().Be(5m);
    }

    [Fact]
    public void From_WithThreeDecimals_Throws()
    {
        // OR-M2 : 4.505 est rejeté (exemple du critère).
        Action act = () => Money.From(4.505m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void From_WithSmallestTwoDecimalAmount_Succeeds()
    {
        // OR-M3 : borne basse — 0.01 est le plus petit montant strictement positif à deux décimales.
        var money = Money.From(0.01m);

        money.Amount.Should().Be(0.01m);
    }

    [Fact]
    public void From_WithZero_Throws()
    {
        // OR-M3 : zéro est rejeté (une gratuité ne s'encode pas dans une commande).
        Action act = () => Money.From(0m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void From_WithNegativeAmount_Throws()
    {
        // OR-M3 : négatif est rejeté.
        Action act = () => Money.From(-4.50m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Addition_IsExact()
    {
        // OR-M4 : addition exacte — 0.10 + 0.20 = 0.30 (inexact en flottant binaire,
        // exact en décimal).
        var sum = Money.From(0.10m) + Money.From(0.20m);

        sum.Amount.Should().Be(0.30m);
    }

    [Fact]
    public void MultiplicationByInteger_IsExact()
    {
        // OR-M4 : multiplication par un entier exacte — 0.10 × 3 = 0.30.
        var product = Money.From(0.10m) * 3;

        product.Amount.Should().Be(0.30m);
    }
}
