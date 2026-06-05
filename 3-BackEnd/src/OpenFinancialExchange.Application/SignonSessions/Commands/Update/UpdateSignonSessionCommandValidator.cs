using FluentValidation;

namespace OpenFinancialExchange.Application.SignonSessions.Commands.Update;

public sealed class UpdateSignonSessionCommandValidator
    : AbstractValidator<UpdateSignonSessionCommand>
{
    public UpdateSignonSessionCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("A valid Id is required.");

        RuleFor(x => x.StatusCode)
            .NotEmpty().WithMessage("Status code is required.")
            .MaximumLength(20).WithMessage("Status code must not exceed 20 characters.");

        RuleFor(x => x.StatusSeverity)
            .NotEmpty().WithMessage("Status severity is required.")
            .MaximumLength(20).WithMessage("Status severity must not exceed 20 characters.");

        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("Language is required.")
            .MaximumLength(10).WithMessage("Language must not exceed 10 characters.");
    }
}
