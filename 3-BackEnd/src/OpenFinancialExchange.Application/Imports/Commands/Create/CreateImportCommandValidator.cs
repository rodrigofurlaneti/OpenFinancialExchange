using FluentValidation;

namespace OpenFinancialExchange.Application.Imports.Commands.Create;

public sealed class CreateImportCommandValidator : AbstractValidator<CreateImportCommand>
{
    public CreateImportCommandValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required.")
            .MaximumLength(500).WithMessage("File name must not exceed 500 characters.");

        RuleFor(x => x.OFXHeader)
            .NotEmpty().WithMessage("OFX header is required.");

        RuleFor(x => x.OFXData)
            .NotEmpty().WithMessage("OFX data is required.");

        RuleFor(x => x.OFXVersion)
            .NotEmpty().WithMessage("OFX version is required.")
            .MaximumLength(10).WithMessage("OFX version must not exceed 10 characters.");

        RuleFor(x => x.OFXSecurity)
            .NotEmpty().WithMessage("OFX security is required.")
            .MaximumLength(50).WithMessage("OFX security must not exceed 50 characters.");

        RuleFor(x => x.OFXEncoding)
            .NotEmpty().WithMessage("OFX encoding is required.")
            .MaximumLength(50).WithMessage("OFX encoding must not exceed 50 characters.");

        RuleFor(x => x.OFXCharset)
            .NotEmpty().WithMessage("OFX charset is required.")
            .MaximumLength(50).WithMessage("OFX charset must not exceed 50 characters.");

        RuleFor(x => x.OFXCompression)
            .NotEmpty().WithMessage("OFX compression is required.")
            .MaximumLength(50).WithMessage("OFX compression must not exceed 50 characters.");

        RuleFor(x => x.OFXOldFileUID)
            .NotEmpty().WithMessage("OFX old file UID is required.")
            .MaximumLength(100).WithMessage("OFX old file UID must not exceed 100 characters.");

        RuleFor(x => x.OFXNewFileUID)
            .NotEmpty().WithMessage("OFX new file UID is required.")
            .MaximumLength(100).WithMessage("OFX new file UID must not exceed 100 characters.");

        RuleFor(x => x.ImportedBy)
            .MaximumLength(200).WithMessage("ImportedBy must not exceed 200 characters.")
            .When(x => x.ImportedBy is not null);
    }
}
