using FluentValidation;

namespace OpenFinancialExchange.Application.LedgerBalances.Commands.Update;

public sealed class UpdateLedgerBalanceCommandValidator
    : AbstractValidator<UpdateLedgerBalanceCommand>
{
    private static readonly string[] AllowedTypes = ["LEDGER", "AVAIL"];

    public UpdateLedgerBalanceCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("A valid Id is required.");

        RuleFor(x => x.BalanceType)
            .NotEmpty().WithMessage("Balance type is required.")
            .Must(t => AllowedTypes.Contains(t))
            .WithMessage($"BalanceType must be one of: {string.Join(", ", AllowedTypes)}.");

        RuleFor(x => x.AsOfDate)
            .NotEmpty().WithMessage("AsOfDate is required.");
    }
}
