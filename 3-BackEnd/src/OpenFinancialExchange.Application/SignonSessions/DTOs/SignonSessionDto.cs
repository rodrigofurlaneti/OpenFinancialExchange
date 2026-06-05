namespace OpenFinancialExchange.Application.SignonSessions.DTOs;

public sealed record SignonSessionDto(
    int Id,
    int ImportId,
    string StatusCode,
    string StatusSeverity,
    string ServerDateRaw,
    DateTime? ServerDate,
    string Language,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
