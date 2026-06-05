using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Domain.Entities;

public sealed class Statement : AggregateRoot
{
    public int      AccountId      { get; private set; }
    public string   TRNUID         { get; private set; } = default!;
    public string   StatusCode     { get; private set; } = default!;
    public string   StatusSeverity { get; private set; } = default!;
    public DateTime StartDate      { get; private set; }
    public DateTime EndDate        { get; private set; }
    public string?  TimeZone       { get; private set; }

    private Statement() { }

    public static Result<Statement> Create(
        int accountId, string trnuid,
        string statusCode, string statusSeverity,
        DateTime startDate, DateTime endDate, string? timeZone)
    {
        if (accountId <= 0)
            return Result.Failure<Statement>(new Error("Statement.InvalidAccount", "Valid Account ID is required."));

        if (string.IsNullOrWhiteSpace(trnuid))
            return Result.Failure<Statement>(new Error("Statement.InvalidTRNUID", "TRNUID is required."));

        if (startDate >= endDate)
            return Result.Failure<Statement>(new Error("Statement.InvalidDateRange", "StartDate must be before EndDate."));

        var entity = new Statement
        {
            AccountId      = accountId,
            TRNUID         = trnuid.Trim(),
            StatusCode     = statusCode.Trim(),
            StatusSeverity = statusSeverity.Trim(),
            StartDate      = startDate,
            EndDate        = endDate,
            TimeZone       = timeZone?.Trim(),
            CreatedAt      = DateTime.UtcNow
        };

        return Result.Success(entity);
    }

    public Result Update(string statusCode, string statusSeverity, DateTime startDate, DateTime endDate, string? timeZone)
    {
        if (startDate >= endDate)
            return Result.Failure(new Error("Statement.InvalidDateRange", "StartDate must be before EndDate."));

        StatusCode     = statusCode.Trim();
        StatusSeverity = statusSeverity.Trim();
        StartDate      = startDate;
        EndDate        = endDate;
        TimeZone       = timeZone?.Trim();
        SetUpdatedAt(DateTime.UtcNow);
        return Result.Success();
    }
}
