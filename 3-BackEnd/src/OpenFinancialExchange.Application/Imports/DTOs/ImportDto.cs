namespace OpenFinancialExchange.Application.Imports.DTOs;

public sealed record ImportDto(
    int Id,
    string FileName,
    DateTime ImportedAt,
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
    string? ImportedBy,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
