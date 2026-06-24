using FluentValidation;

namespace OpenFinancialExchange.Application.Features.FinancialInstitutions.Create;

public sealed class CreateFinancialInstitutionCommandValidator : AbstractValidator<CreateFinancialInstitutionCommand>
{
    public CreateFinancialInstitutionCommandValidator()
    {
        RuleFor(x => x.BankId)
            .NotEmpty().WithMessage("BankId is required.")
            .MaximumLength(20).WithMessage("BankId must not exceed 20 characters.");

        RuleFor(x => x.OrgName)
            .MaximumLength(100).When(x => x.OrgName is not null)
            .WithMessage("OrgName must not exceed 100 characters.");

        RuleFor(x => x.Fid)
            .MaximumLength(50).When(x => x.Fid is not null)
            .WithMessage("Fid must not exceed 50 characters.");
    }
}
