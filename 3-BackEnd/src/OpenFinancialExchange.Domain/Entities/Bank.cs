using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Domain.Entities;

public sealed class Bank : AggregateRoot
{
    public string  COMPECode { get; private set; } = default!;
    public string  BankName  { get; private set; } = default!;
    public string? ISPB      { get; private set; }
    public bool    IsActive  { get; private set; }

    private Bank() { }

    public static Result<Bank> Create(string compeCode, string bankName, string? ispb)
    {
        if (string.IsNullOrWhiteSpace(compeCode))
            return Result.Failure<Bank>(new Error("Bank.InvalidCode", "COMPE code is required."));

        if (string.IsNullOrWhiteSpace(bankName))
            return Result.Failure<Bank>(new Error("Bank.InvalidName", "Bank name is required."));

        var entity = new Bank
        {
            COMPECode = compeCode.Trim(),
            BankName  = bankName.Trim(),
            ISPB      = ispb?.Trim(),
            IsActive  = true,
            CreatedAt = DateTime.UtcNow
        };

        return Result.Success(entity);
    }

    public Result Update(string bankName, string? ispb)
    {
        if (string.IsNullOrWhiteSpace(bankName))
            return Result.Failure(new Error("Bank.InvalidName", "Bank name is required."));

        BankName = bankName.Trim();
        ISPB     = ispb?.Trim();
        SetUpdatedAt(DateTime.UtcNow);
        return Result.Success();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt(DateTime.UtcNow);
    }
}
