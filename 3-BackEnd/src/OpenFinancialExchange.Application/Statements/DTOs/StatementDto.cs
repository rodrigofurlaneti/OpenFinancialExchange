namespace OpenFinancialExchange.Application.Statements.DTOs;

public sealed record StatementDto(
    int Id,
    int AccountId,
    string TRNUID,
    string StatusCode,
    string StatusSeverity,
    DateTime StartDate,
    DateTime EndDate,
    string? TimeZone,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
