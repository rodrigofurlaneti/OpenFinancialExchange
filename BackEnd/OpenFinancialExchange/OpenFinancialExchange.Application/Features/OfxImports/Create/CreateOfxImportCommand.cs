using OpenFinancialExchange.Application.Abstractions.Messaging;

namespace OpenFinancialExchange.Application.Features.OfxImports.Create;

public sealed record CreateOfxImportCommand(
    long? BankAccountId,
    string FileName,
    string FileHash,
    short? OfxHeaderVersion,
    short? OfxVersion,
    string? OfxData,
    string? Encoding,
    string? Charset,
    string? Security,
    string? Compression,
    string? OldFileUid,
    string? NewFileUid) : ICommand<long>;
