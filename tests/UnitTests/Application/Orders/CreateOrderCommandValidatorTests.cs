using EventsManager.Application.Orders;
using EventsManager.Domain.Orders;
using FluentValidation.TestHelper;
using DomainOrder = EventsManager.Domain.Orders.Order;

namespace EventsManager.UnitTests.Application.Orders;

public class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_Passes()
    {
        var result = _validator.TestValidate(ValidCommand());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyEventId_Fails()
    {
        // OR-C1 : une commande référence un évènement — une référence vide est rejetée.
        var command = ValidCommand() with { EventId = Guid.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.EventId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithInvalidCustomerName_Fails(string? customerName)
    {
        // OR-C2 : nom du client obligatoire, non vide (ni blanc).
        var command = ValidCommand() with { CustomerName = customerName! };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.CustomerName);
    }

    [Fact]
    public void Validate_WithCustomerNameOfMaxLength_Passes()
    {
        // OR-C2 : borne exacte — 100 caractères est la dernière longueur valide.
        var command = ValidCommand() with { CustomerName = new string('a', DomainOrder.CustomerNameMaxLength) };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithCustomerNameLongerThanMaxLength_Fails()
    {
        // OR-C2 : borne exacte — 101 caractères est la première longueur invalide.
        var command = ValidCommand() with { CustomerName = new string('a', DomainOrder.CustomerNameMaxLength + 1) };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.CustomerName);
    }

    [Fact]
    public void Validate_WithoutLines_Fails()
    {
        // OR-C3 : au moins 1 ligne.
        var command = ValidCommand() with { Lines = [] };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Lines);
    }

    [Fact]
    public void Validate_WithDuplicateLabels_Fails()
    {
        // OR-C4 : deux lignes de même libellé sont interdites.
        var command = ValidCommand() with
        {
            Lines =
            [
                new CreateOrderCommand.Line("Frites", 4.00m, 2),
                new CreateOrderCommand.Line("Frites", 1.50m, 3),
            ],
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Lines);
    }

    [Fact]
    public void Validate_WithDuplicateLabelsIgnoringCase_Fails()
    {
        // OR-C4 : le doublon s'apprécie insensiblement à la casse.
        var command = ValidCommand() with
        {
            Lines =
            [
                new CreateOrderCommand.Line("frites", 4.00m, 2),
                new CreateOrderCommand.Line("FRITES", 1.50m, 3),
            ],
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Lines);
    }

    [Fact]
    public void Validate_WithDuplicateLabelsAfterTrim_Fails()
    {
        // OR-C4 : le doublon s'apprécie après trim.
        var command = ValidCommand() with
        {
            Lines =
            [
                new CreateOrderCommand.Line("Frites", 4.00m, 2),
                new CreateOrderCommand.Line("  Frites  ", 1.50m, 3),
            ],
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Lines);
    }

    [Fact]
    public void Validate_WithDistinctLabels_Passes()
    {
        // OR-C4 : cas positif — des libellés distincts sont acceptés.
        var command = ValidCommand() with
        {
            Lines =
            [
                new CreateOrderCommand.Line("Frites", 4.00m, 2),
                new CreateOrderCommand.Line("Bière", 1.50m, 3),
            ],
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithInvalidLineLabel_Fails(string? label)
    {
        // OR-L1 : libellé obligatoire, non vide (ni blanc).
        var command = ValidCommand() with { Lines = [new CreateOrderCommand.Line(label!, 4.50m, 2)] };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Lines[0].Label");
    }

    [Fact]
    public void Validate_WithLineLabelOfMaxLength_Passes()
    {
        // OR-L1 : borne exacte — 100 caractères est la dernière longueur valide.
        var label = new string('a', OrderLine.LabelMaxLength);
        var command = ValidCommand() with { Lines = [new CreateOrderCommand.Line(label, 4.50m, 2)] };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithLineLabelLongerThanMaxLength_Fails()
    {
        // OR-L1 : borne exacte — 101 caractères est la première longueur invalide.
        var label = new string('a', OrderLine.LabelMaxLength + 1);
        var command = ValidCommand() with { Lines = [new CreateOrderCommand.Line(label, 4.50m, 2)] };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Lines[0].Label");
    }

    [Fact]
    public void Validate_WithZeroUnitPrice_Fails()
    {
        // OR-L2 / OR-M3 : prix strictement positif — zéro est rejeté.
        var command = ValidCommand() with { Lines = [new CreateOrderCommand.Line("Frites", 0m, 2)] };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Lines[0].UnitPrice");
    }

    [Fact]
    public void Validate_WithNegativeUnitPrice_Fails()
    {
        // OR-L2 / OR-M3 : prix strictement positif — négatif est rejeté.
        var command = ValidCommand() with { Lines = [new CreateOrderCommand.Line("Frites", -4.50m, 2)] };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Lines[0].UnitPrice");
    }

    [Fact]
    public void Validate_WithThreeDecimalsUnitPrice_Fails()
    {
        // OR-L2 / OR-M2 : deux décimales maximum — 4.505 est rejeté (exemple du critère).
        var command = ValidCommand() with { Lines = [new CreateOrderCommand.Line("Frites", 4.505m, 2)] };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Lines[0].UnitPrice");
    }

    [Fact]
    public void Validate_WithTwoDecimalsUnitPrice_Passes()
    {
        // OR-L2 / OR-M2 : 4.50 est valide (exemple du critère).
        var command = ValidCommand() with { Lines = [new CreateOrderCommand.Line("Frites", 4.50m, 2)] };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(20)]
    public void Validate_WithQuantityWithinBounds_Passes(int quantity)
    {
        // OR-L3 : bornes exactes — 1 et 20 sont valides.
        var command = ValidCommand() with { Lines = [new CreateOrderCommand.Line("Frites", 4.50m, quantity)] };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(21)]
    public void Validate_WithQuantityOutOfBounds_Fails(int quantity)
    {
        // OR-L3 : bornes exactes — 0 et 21 sont les premières valeurs invalides.
        var command = ValidCommand() with { Lines = [new CreateOrderCommand.Line("Frites", 4.50m, quantity)] };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Lines[0].Quantity");
    }

    private static CreateOrderCommand ValidCommand()
    {
        return new CreateOrderCommand(
            Guid.CreateVersion7(),
            "Alice Dupont",
            [new CreateOrderCommand.Line("Frites", 4.50m, 2)]);
    }
}
