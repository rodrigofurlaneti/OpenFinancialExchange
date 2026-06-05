namespace OpenFinancialExchange.Application.Banks.DTOs;

public sealed record BankDto(
    int Id,
    string COMPECode,
    string BankName,
    string? ISPB,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
