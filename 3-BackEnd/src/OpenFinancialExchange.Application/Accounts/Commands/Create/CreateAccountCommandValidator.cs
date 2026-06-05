using FluentValidation;

namespace OpenFinancialExchange.Application.Accounts.Commands.Create;

public sealed class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    private static readonly string[] AllowedTypes = ["CHECKING", "SAVINGS", "MONEYMRKT", "CREDITLINE"];

    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.ImportId)
            .GreaterThan(0).WithMessage("A valid Import ID is required.");

        RuleFor(x => x.BankId)
            .GreaterThan(0).WithMessage("A valid Bank ID is required.");

        RuleFor(x => x.BranchNumber)
            .MaximumLength(20).WithMessage("Branch number must not exceed 20 characters.")
            .When(x => x.BranchNumber is not null);

        RuleFor(x => x.AccountNumber)
            .NotEmpty().WithMessage("Account number is required.")
            .MaximumLength(50).WithMessage("Account number must not exceed 50 characters.");

        RuleFor(x => x.AccountType)
            .NotEmpty().WithMessage("Account type is required.")
            .Must(t => AllowedTypes.Contains(t))
            .WithMessage($"AccountType must be one of: {string.Join(", ", AllowedTypes)}.");

        RuleFor(x => x.DefaultCurrency)
            .NotEmpty().WithMessage("Default currency is required.")
            .Length(3).WithMessage("Default currency must be exactly 3 characters.");
    }
}
