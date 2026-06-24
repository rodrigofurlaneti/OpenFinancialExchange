namespace OpenFinancialExchange.Application.Features.OfxStatements;

public sealed record OfxStatementResponse(
    long Id,
    long ImportId,
    long BankAccountId,
    string? TrnUid,
    string CurDef,
    DateTime? DtServer,
    string? Language,
    short? StatusCode,
    string? StatusSeverity,
    DateTime? DtStart,
    DateTime? DtEnd,
    DateTime CreatedAt);
