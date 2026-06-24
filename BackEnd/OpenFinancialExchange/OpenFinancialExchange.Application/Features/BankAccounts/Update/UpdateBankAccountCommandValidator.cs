using FluentValidation;

namespace OpenFinancialExchange.Application.Features.BankAccounts.Update;

public sealed class UpdateBankAccountCommandValidator : AbstractValidator<UpdateBankAccountCommand>
{
    private static readonly string[] ValidAcctTypes =
        ["CHECKING", "SAVINGS", "MONEYMRKT", "CREDITLINE", "CD", "OTHER"];

    public UpdateBankAccountCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.BranchId).MaximumLength(20).When(x => x.BranchId is not null);
        RuleFor(x => x.AcctType).NotEmpty()
            .Must(t => ValidAcctTypes.Contains(t?.ToUpperInvariant()))
            .WithMessage($"AcctType must be one of: {string.Join(", ", ValidAcctTypes)}.");
    }
}
