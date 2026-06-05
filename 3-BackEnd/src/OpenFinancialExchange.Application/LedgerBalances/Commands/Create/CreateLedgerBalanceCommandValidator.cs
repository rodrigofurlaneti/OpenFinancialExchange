using FluentValidation;

namespace OpenFinancialExchange.Application.LedgerBalances.Commands.Create;

public sealed class CreateLedgerBalanceCommandValidator
    : AbstractValidator<CreateLedgerBalanceCommand>
{
    private static readonly string[] AllowedTypes = ["LEDGER", "AVAIL"];

    public CreateLedgerBalanceCommandValidator()
    {
        RuleFor(x => x.StatementId)
            .GreaterThan(0).WithMessage("A valid Statement ID is required.");

        RuleFor(x => x.BalanceType)
            .NotEmpty().WithMessage("Balance type is required.")
            .Must(t => AllowedTypes.Contains(t))
            .WithMessage($"BalanceType must be one of: {string.Join(", ", AllowedTypes)}.");

        RuleFor(x => x.AsOfDate)
            .NotEmpty().WithMessage("AsOfDate is required.");
    }
}
