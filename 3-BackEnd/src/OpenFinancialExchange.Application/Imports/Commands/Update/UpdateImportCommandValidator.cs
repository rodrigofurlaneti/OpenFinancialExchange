using FluentValidation;

namespace OpenFinancialExchange.Application.Imports.Commands.Update;

public sealed class UpdateImportCommandValidator : AbstractValidator<UpdateImportCommand>
{
    public UpdateImportCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("A valid Id is required.");
    }
}
