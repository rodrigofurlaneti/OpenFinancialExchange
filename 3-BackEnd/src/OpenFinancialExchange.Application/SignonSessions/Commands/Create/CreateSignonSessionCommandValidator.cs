using FluentValidation;

namespace OpenFinancialExchange.Application.SignonSessions.Commands.Create;

public sealed class CreateSignonSessionCommandValidator
    : AbstractValidator<CreateSignonSessionCommand>
{
    public CreateSignonSessionCommandValidator()
    {
        RuleFor(x => x.ImportId)
            .GreaterThan(0).WithMessage("A valid Import ID is required.");

        RuleFor(x => x.StatusCode)
            .NotEmpty().WithMessage("Status code is required.")
            .MaximumLength(20).WithMessage("Status code must not exceed 20 characters.");

        RuleFor(x => x.StatusSeverity)
            .NotEmpty().WithMessage("Status severity is required.")
            .MaximumLength(20).WithMessage("Status severity must not exceed 20 characters.");

        RuleFor(x => x.ServerDateRaw)
            .NotEmpty().WithMessage("Server date raw is required.")
            .MaximumLength(30).WithMessage("Server date raw must not exceed 30 characters.");

        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("Language is required.")
            .MaximumLength(10).WithMessage("Language must not exceed 10 characters.");
    }
}
