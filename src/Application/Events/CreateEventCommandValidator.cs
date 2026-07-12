using FluentValidation;

namespace EventsManager.Application.Events;

public sealed class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    private readonly TimeProvider _timeProvider;

    public CreateEventCommandValidator(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;

        // NotEmpty rejette null, la chaîne vide et les chaînes blanches (doc FluentValidation).
        RuleFor(command => command.Name)
            .NotEmpty()
            .WithMessage("Le nom de l'évènement ne peut pas être vide.");

        RuleFor(command => command.Date)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("La date de l'évènement ne peut pas être vide.")
            .Must(date => date > _timeProvider.TodayLocal())
            .WithMessage("La date de l'évènement doit être au moins à J+1.");
    }
}
