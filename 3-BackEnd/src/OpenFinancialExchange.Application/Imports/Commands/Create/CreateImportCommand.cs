using MediatR;
using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Application.Imports.Commands.Create;

public sealed record CreateImportCommand(
    string FileName,
    string OFXHeader,
    string OFXData,
    string OFXVersion,
    string OFXSecurity,
    string OFXEncoding,
    string OFXCharset,
    string OFXCompression,
    string OFXOldFileUID,
    string OFXNewFileUID,
    string? Notes,
    string? ImportedBy) : IRequest<Result<int>>;
