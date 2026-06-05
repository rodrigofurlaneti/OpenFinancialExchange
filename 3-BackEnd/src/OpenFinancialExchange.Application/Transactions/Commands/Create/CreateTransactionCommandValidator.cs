using FluentValidation;

namespace OpenFinancialExchange.Application.Transactions.Commands.Create;

public sealed class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    private static readonly string[] AllowedTypes =
    [
        "CREDIT", "DEBIT", "INT", "DIV", "FEE", "SRVCHG",
        "DEP", "ATM", "POS", "XFER", "CHECK", "PAYMENT",
        "CASH", "DIRECTDEP", "DIRECTDEBIT", "REPEATPMT", "OTHER"
    ];

    public CreateTransactionCommandValidator()
    {
        RuleFor(x => x.StatementId)
            .GreaterThan(0).WithMessage("A valid Statement ID is required.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category ID must be greater than 0 when provided.")
            .When(x => x.CategoryId.HasValue);

        RuleFor(x => x.TransactionType)
            .NotEmpty().WithMessage("Transaction type is required.")
            .Must(t => AllowedTypes.Contains(t))
            .WithMessage($"TransactionType must be one of: {string.Join(", ", AllowedTypes)}.");

        RuleFor(x => x.PostedDateRaw)
            .NotEmpty().WithMessage("Posted date raw is required.")
            .MaximumLength(30).WithMessage("Posted date raw must not exceed 30 characters.");

        RuleFor(x => x.FITID)
            .NotEmpty().WithMessage("FITID is required.")
            .MaximumLength(100).WithMessage("FITID must not exceed 100 characters.");

        RuleFor(x => x.CheckNumber)
            .MaximumLength(20).WithMessage("Check number must not exceed 20 characters.")
            .When(x => x.CheckNumber is not null);

        RuleFor(x => x.Memo)
            .MaximumLength(500).WithMessage("Memo must not exceed 500 characters.")
            .When(x => x.Memo is not null);

        RuleFor(x => x.PayeeName)
            .MaximumLength(200).WithMessage("Payee name must not exceed 200 characters.")
            .When(x => x.PayeeName is not null);

        RuleFor(x => x.OperationSubtype)
            .MaximumLength(50).WithMessage("Operation subtype must not exceed 50 characters.")
            .When(x => x.OperationSubtype is not null);
    }
}
