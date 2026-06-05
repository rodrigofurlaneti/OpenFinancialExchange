using FluentValidation;

namespace OpenFinancialExchange.Application.Statements.Commands.Update;

public sealed class UpdateStatementCommandValidator : AbstractValidator<UpdateStatementCommand>
{
    public UpdateStatementCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("A valid Id is required.");

        RuleFor(x => x.StatusCode)
            .NotEmpty().WithMessage("Status code is required.")
            .MaximumLength(20).WithMessage("Status code must not exceed 20 characters.");

        RuleFor(x => x.StatusSeverity)
            .NotEmpty().WithMessage("Status severity is required.")
            .MaximumLength(20).WithMessage("Status severity must not exceed 20 characters.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.")
            .LessThan(x => x.EndDate).WithMessage("Start date must be before end date.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required.");

        RuleFor(x => x.TimeZone)
            .MaximumLength(50).WithMessage("Time zone must not exceed 50 characters.")
            .When(x => x.TimeZone is not null);
    }
}
