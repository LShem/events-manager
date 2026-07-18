using EventsManager.Domain.Events;
using EventsManager.Domain.Orders;

namespace EventsManager.UnitTests.Domain.Orders;

public class OrderTests
{
    private static readonly EventId SomeEventId = EventId.New();
    private static readonly DateTime CreatedAtUtc = new(2026, 7, 13, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void CustomerNameMaxLength_MatchesAcceptanceCriteria()
    {
        // OR-C2 : nom du client limité à 100 caractères.
        Order.CustomerNameMaxLength.Should().Be(100);
    }

    [Fact]
    public void Create_WithValidData_SetsPropertiesAndAllLines()
    {
        var lines = new[] { Line("Frites", 4.00m, 2), Line("Bière", 1.50m, 3) };

        var order = Order.Create(SomeEventId, "Alice Dupont", lines, CreatedAtUtc);

        // OR-C7 : identifiant typé généré à la création.
        order.Id.Should().NotBe(default(OrderId));
        order.EventId.Should().Be(SomeEventId);
        order.CustomerName.Should().Be("Alice Dupont");
        order.CreatedAt.Should().Be(CreatedAtUtc);
        // OR-C6 : la commande est créée complète — la racine et toutes ses lignes.
        order.Lines.Should().Equal(lines);
    }

    [Fact]
    public void Create_Trims_CustomerName()
    {
        // OR-C2 : trim appliqué au nom du client.
        var order = Order.Create(SomeEventId, "  Alice Dupont  ", [Line("Frites", 4.00m, 2)], CreatedAtUtc);

        order.CustomerName.Should().Be("Alice Dupont");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidCustomerName_Throws(string? customerName)
    {
        // OR-C2 : nom du client obligatoire, non vide (ni blanc).
        Action act = () => Order.Create(SomeEventId, customerName!, [Line("Frites", 4.00m, 2)], CreatedAtUtc);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithCustomerNameOfMaxLength_Succeeds()
    {
        // OR-C2 : borne exacte — 100 caractères est la dernière longueur valide.
        var customerName = new string('a', Order.CustomerNameMaxLength);

        var order = Order.Create(SomeEventId, customerName, [Line("Frites", 4.00m, 2)], CreatedAtUtc);

        order.CustomerName.Should().Be(customerName);
    }

    [Fact]
    public void Create_WithCustomerNameTrimmedToMaxLength_Succeeds()
    {
        // OR-C2 : la limite se mesure après trim.
        var customerName = $"  {new string('a', Order.CustomerNameMaxLength)}  ";

        var order = Order.Create(SomeEventId, customerName, [Line("Frites", 4.00m, 2)], CreatedAtUtc);

        order.CustomerName.Should().Be(customerName.Trim());
    }

    [Fact]
    public void Create_WithCustomerNameLongerThanMaxLength_Throws()
    {
        // OR-C2 : borne exacte — 101 caractères est la première longueur invalide.
        var customerName = new string('a', Order.CustomerNameMaxLength + 1);

        Action act = () => Order.Create(SomeEventId, customerName, [Line("Frites", 4.00m, 2)], CreatedAtUtc);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithDefaultEventId_Throws()
    {
        // OR-C1 : une commande référence un évènement — une référence vide est rejetée
        // (l'existence de l'évènement se vérifie dans la couche Application).
        Action act = () => Order.Create(default, "Alice Dupont", [Line("Frites", 4.00m, 2)], CreatedAtUtc);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithoutLines_Throws()
    {
        // OR-C3 : au moins 1 ligne — zéro ligne est rejeté.
        Action act = () => Order.Create(SomeEventId, "Alice Dupont", [], CreatedAtUtc);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNullLines_Throws()
    {
        // OR-C3 : au moins 1 ligne — l'absence de collection est rejetée.
        Action act = () => Order.Create(SomeEventId, "Alice Dupont", null!, CreatedAtUtc);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithSingleLine_Succeeds()
    {
        // OR-C3 : borne exacte — 1 ligne est le minimum valide.
        var order = Order.Create(SomeEventId, "Alice Dupont", [Line("Frites", 4.00m, 2)], CreatedAtUtc);

        order.Lines.Should().ContainSingle();
    }

    [Fact]
    public void Create_WithManyLines_Succeeds()
    {
        // OR-C3 : pas de plafond numérique — 50 lignes à libellés distincts passent
        // (la borne naturelle viendra du catalogue de produits, tranche future).
        var lines = Enumerable.Range(1, 50).Select(i => Line($"Produit {i}", 1.00m, 1)).ToArray();

        var order = Order.Create(SomeEventId, "Alice Dupont", lines, CreatedAtUtc);

        order.Lines.Should().HaveCount(50);
    }

    [Fact]
    public void Create_WithDuplicateLabels_Throws()
    {
        // OR-C4 : deux lignes de même libellé sont interdites dans une même commande.
        var lines = new[] { Line("Frites", 4.00m, 2), Line("Frites", 1.50m, 3) };

        Action act = () => Order.Create(SomeEventId, "Alice Dupont", lines, CreatedAtUtc);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithDuplicateLabelsIgnoringCase_Throws()
    {
        // OR-C4 : le doublon s'apprécie insensiblement à la casse.
        var lines = new[] { Line("frites", 4.00m, 2), Line("FRITES", 1.50m, 3) };

        Action act = () => Order.Create(SomeEventId, "Alice Dupont", lines, CreatedAtUtc);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithDuplicateLabelsAfterTrim_Throws()
    {
        // OR-C4 : le doublon s'apprécie après trim — une saisie avec espaces parasites
        // retombe sur le même libellé.
        var lines = new[] { Line("Frites", 4.00m, 2), Line("  Frites  ", 1.50m, 3) };

        Action act = () => Order.Create(SomeEventId, "Alice Dupont", lines, CreatedAtUtc);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithDistinctLabels_Succeeds()
    {
        // OR-C4 : cas positif — des libellés distincts sont acceptés.
        var lines = new[] { Line("Frites", 4.00m, 2), Line("Bière", 1.50m, 3) };

        var order = Order.Create(SomeEventId, "Alice Dupont", lines, CreatedAtUtc);

        order.Lines.Should().HaveCount(2);
    }

    [Fact]
    public void Total_IsSumOfLineSubtotals()
    {
        // OR-C5 : total = somme des sous-totaux — 2 × 4.00 + 3 × 1.50 = 12.50
        // (exemple exact du critère).
        var order = Order.Create(
            SomeEventId,
            "Alice Dupont",
            [Line("Frites", 4.00m, 2), Line("Bière", 1.50m, 3)],
            CreatedAtUtc);

        order.Total.Amount.Should().Be(12.50m);
    }

    [Fact]
    public void Create_Twice_GeneratesDistinctIds()
    {
        // OR-C7 : l'identifiant est généré à la création — deux commandes, deux identités.
        var first = Order.Create(SomeEventId, "Alice Dupont", [Line("Frites", 4.00m, 2)], CreatedAtUtc);
        var second = Order.Create(SomeEventId, "Bruno Petit", [Line("Bière", 1.50m, 3)], CreatedAtUtc);

        first.Id.Should().NotBe(second.Id);
    }

    private static OrderLine Line(string label, decimal unitPrice, int quantity)
    {
        return OrderLine.Create(label, Money.From(unitPrice), quantity);
    }
}
