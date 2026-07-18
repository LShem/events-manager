using EventsManager.Domain.Orders;

namespace EventsManager.UnitTests.Domain.Orders;

public class OrderLineTests
{
    private static readonly Money ValidPrice = Money.From(4.50m);

    [Fact]
    public void Constants_MatchAcceptanceCriteria()
    {
        // OR-L1 : libellé 100 caractères maximum ; OR-L3 : quantité minimum 1, maximum 20.
        OrderLine.LabelMaxLength.Should().Be(100);
        OrderLine.QuantityMin.Should().Be(1);
        OrderLine.QuantityMax.Should().Be(20);
    }

    [Fact]
    public void Create_WithValidData_SetsProperties()
    {
        var line = OrderLine.Create("Frites", ValidPrice, 3);

        line.Label.Should().Be("Frites");
        line.UnitPrice.Should().Be(ValidPrice);
        line.Quantity.Should().Be(3);
    }

    [Fact]
    public void Create_Trims_Label()
    {
        // OR-L1 : trim appliqué au libellé.
        var line = OrderLine.Create("  Frites  ", ValidPrice, 3);

        line.Label.Should().Be("Frites");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidLabel_Throws(string? label)
    {
        // OR-L1 : libellé obligatoire, non vide (ni blanc).
        Action act = () => OrderLine.Create(label!, ValidPrice, 3);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithLabelOfMaxLength_Succeeds()
    {
        // OR-L1 : borne exacte — 100 caractères est la dernière longueur valide.
        var label = new string('a', OrderLine.LabelMaxLength);

        var line = OrderLine.Create(label, ValidPrice, 3);

        line.Label.Should().Be(label);
    }

    [Fact]
    public void Create_WithLabelTrimmedToMaxLength_Succeeds()
    {
        // OR-L1 : la limite se mesure après trim.
        var label = $"  {new string('a', OrderLine.LabelMaxLength)}  ";

        var line = OrderLine.Create(label, ValidPrice, 3);

        line.Label.Should().Be(label.Trim());
    }

    [Fact]
    public void Create_WithLabelLongerThanMaxLength_Throws()
    {
        // OR-L1 : borne exacte — 101 caractères est la première longueur invalide.
        var label = new string('a', OrderLine.LabelMaxLength + 1);

        Action act = () => OrderLine.Create(label, ValidPrice, 3);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithDefaultUnitPrice_Throws()
    {
        // OR-L2 : le prix unitaire est un Money (strictement positif) — un Money
        // non construit (default, montant zéro) contournerait l'invariant : rejeté.
        Action act = () => OrderLine.Create("Frites", default, 3);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithQuantityAtMin_Succeeds()
    {
        // OR-L3 : borne exacte — 1 est la première quantité valide.
        var line = OrderLine.Create("Frites", ValidPrice, OrderLine.QuantityMin);

        line.Quantity.Should().Be(1);
    }

    [Fact]
    public void Create_WithQuantityAtMax_Succeeds()
    {
        // OR-L3 : borne exacte — 20 est la dernière quantité valide.
        var line = OrderLine.Create("Frites", ValidPrice, OrderLine.QuantityMax);

        line.Quantity.Should().Be(20);
    }

    [Fact]
    public void Create_WithQuantityBelowMin_Throws()
    {
        // OR-L3 : borne exacte — 0 est la première quantité invalide sous le minimum.
        Action act = () => OrderLine.Create("Frites", ValidPrice, OrderLine.QuantityMin - 1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithQuantityAboveMax_Throws()
    {
        // OR-L3 : borne exacte — 21 est la première quantité invalide au-dessus du maximum.
        Action act = () => OrderLine.Create("Frites", ValidPrice, OrderLine.QuantityMax + 1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Subtotal_IsUnitPriceTimesQuantity()
    {
        // OR-L4 : sous-total de ligne = prix unitaire × quantité (4.50 × 3 = 13.50).
        var line = OrderLine.Create("Frites", Money.From(4.50m), 3);

        line.Subtotal.Amount.Should().Be(13.50m);
    }
}
