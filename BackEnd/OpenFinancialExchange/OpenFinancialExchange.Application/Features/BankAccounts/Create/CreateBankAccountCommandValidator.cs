using FluentValidation;

namespace OpenFinancialExchange.Application.Features.BankAccounts.Create;

public sealed class CreateBankAccountCommandValidator : AbstractValidator<CreateBankAccountCommand>
{
    private static readonly string[] ValidAcctTypes =
        ["CHECKING", "SAVINGS", "MONEYMRKT", "CREDITLINE", "CD", "OTHER"];

    public CreateBankAccountCommandValidator()
    {
        RuleFor(x => x.FinancialInstitutionId).GreaterThan(0).WithMessage("FinancialInstitutionId must be greater than 0.");
        RuleFor(x => x.BankId).NotEmpty().MaximumLength(20);
        RuleFor(x => x.BranchId).MaximumLength(20).When(x => x.BranchId is not null);
        RuleFor(x => x.AcctId).NotEmpty().MaximumLength(50);
        RuleFor(x => x.AcctType).NotEmpty()
            .Must(t => ValidAcctTypes.Contains(t?.ToUpperInvariant()))
            .WithMessage($"AcctType must be one of: {string.Join(", ", ValidAcctTypes)}.");
    }
}
