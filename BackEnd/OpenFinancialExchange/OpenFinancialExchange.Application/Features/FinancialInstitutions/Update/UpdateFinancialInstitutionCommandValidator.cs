using FluentValidation;

namespace OpenFinancialExchange.Application.Features.FinancialInstitutions.Update;

public sealed class UpdateFinancialInstitutionCommandValidator : AbstractValidator<UpdateFinancialInstitutionCommand>
{
    public UpdateFinancialInstitutionCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id must be greater than 0.");

        RuleFor(x => x.OrgName)
            .MaximumLength(100).When(x => x.OrgName is not null)
            .WithMessage("OrgName must not exceed 100 characters.");

        RuleFor(x => x.Fid)
            .MaximumLength(50).When(x => x.Fid is not null)
            .WithMessage("Fid must not exceed 50 characters.");
    }
}
