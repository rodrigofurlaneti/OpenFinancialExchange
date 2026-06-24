namespace OpenFinancialExchange.Application.Features.OfxImports;

public sealed record OfxImportResponse(
    long Id,
    string FileName,
    string FileHash,
    short? OfxHeaderVersion,
    short? OfxVersion,
    string? OfxData,
    string? Encoding,
    string? Charset,
    string? Security,
    DateTime ImportedAt);
