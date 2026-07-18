using EventsManager.Domain.Orders;
using FluentValidation;

namespace EventsManager.Application.Orders;

/// <summary>
/// Défense en profondeur : duplique les règles du domaine (adossées à ses constantes)
/// pour produire des erreurs groupées plutôt qu'une exception à la première violation.
/// L'existence de l'évènement se vérifie dans le handler : les validators sont
/// enregistrés en singleton, les repositories en scoped (captive dependency sinon).
/// Constructeur classique (pas de primary constructor) : les RuleFor doivent
/// s'enregistrer dans un corps de constructeur — exception documentée dans CLAUDE.md.
/// </summary>
public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(command => command.EventId)
            .NotEmpty()
            .WithMessage("L'identifiant de l'évènement ne peut pas être vide.");

        // NotEmpty rejette null, la chaîne vide et les chaînes blanches (doc FluentValidation).
        // MaximumLength mesure la chaîne brute (avant le trim du domaine) : plus strict,
        // donc aucun nom invalide ne passe.
        RuleFor(command => command.CustomerName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Le nom du client ne peut pas être vide.")
            .MaximumLength(Order.CustomerNameMaxLength)
            .WithMessage($"Le nom du client ne peut pas dépasser {Order.CustomerNameMaxLength} caractères.");

        RuleFor(command => command.Lines)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Une commande doit contenir au moins une ligne.")
            .Must(HaveDistinctLabels)
            .WithMessage("Deux lignes d'une même commande ne peuvent pas porter le même libellé.");

        RuleForEach(command => command.Lines).SetValidator(new LineValidator());
    }

    // Doublons au sens du domaine : après trim, insensible à la casse. Les libellés
    // blancs sont ignorés ici — c'est le NotEmpty par ligne qui les signale.
    private static bool HaveDistinctLabels(IReadOnlyList<CreateOrderCommand.Line> lines)
    {
        var seenLabels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        return lines
            .Select(line => line.Label)
            .Where(label => !string.IsNullOrWhiteSpace(label))
            .Select(label => label.Trim())
            .All(seenLabels.Add);
    }

    private sealed class LineValidator : AbstractValidator<CreateOrderCommand.Line>
    {
        public LineValidator()
        {
            RuleFor(line => line.Label)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Le libellé d'une ligne ne peut pas être vide.")
                .MaximumLength(OrderLine.LabelMaxLength)
                .WithMessage($"Le libellé d'une ligne ne peut pas dépasser {OrderLine.LabelMaxLength} caractères.");

            RuleFor(line => line.UnitPrice)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0)
                .WithMessage("Le prix unitaire d'une ligne doit être strictement positif.")
                .Must(HaveAtMostTwoDecimals)
                .WithMessage("Le prix unitaire d'une ligne ne peut pas porter plus de deux décimales.");

            RuleFor(line => line.Quantity)
                .InclusiveBetween(OrderLine.QuantityMin, OrderLine.QuantityMax)
                .WithMessage($"La quantité d'une ligne doit être comprise entre {OrderLine.QuantityMin} et {OrderLine.QuantityMax}.");
        }

        private static bool HaveAtMostTwoDecimals(decimal unitPrice)
        {
            return decimal.Round(unitPrice, Money.MaxDecimalPlaces) == unitPrice;
        }
    }
}
