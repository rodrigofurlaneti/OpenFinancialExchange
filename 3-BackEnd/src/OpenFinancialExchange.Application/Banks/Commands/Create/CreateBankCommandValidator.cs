using FluentValidation;

namespace OpenFinancialExchange.Application.Banks.Commands.Create;

public sealed class CreateBankCommandValidator : AbstractValidator<CreateBankCommand>
{
    public CreateBankCommandValidator()
    {
        RuleFor(x => x.COMPECode)
            .NotEmpty().WithMessage("COMPE code is required.")
            .MaximumLength(10).WithMessage("COMPE code must not exceed 10 characters.");

        RuleFor(x => x.BankName)
            .NotEmpty().WithMessage("Bank name is required.")
            .MaximumLength(200).WithMessage("Bank name must not exceed 200 characters.");

        RuleFor(x => x.ISPB)
            .MaximumLength(20).WithMessage("ISPB must not exceed 20 characters.")
            .When(x => x.ISPB is not null);
    }
}
