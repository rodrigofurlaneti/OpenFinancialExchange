using OpenFinancialExchange.Domain.Common;

namespace OpenFinancialExchange.Domain.Entities;

public sealed class SignonSession : Entity
{
    public int       ImportId        { get; private set; }
    public string    StatusCode      { get; private set; } = default!;
    public string    StatusSeverity  { get; private set; } = default!;
    public string    ServerDateRaw   { get; private set; } = default!;
    public DateTime? ServerDate      { get; private set; }
    public string    Language        { get; private set; } = default!;

    private SignonSession() { }

    public static Result<SignonSession> Create(
        int importId,
        string statusCode,
        string statusSeverity,
        string serverDateRaw,
        DateTime? serverDate,
        string language)
    {
        if (importId <= 0)
            return Result.Failure<SignonSession>(new Error("SignonSession.InvalidImport", "Valid Import ID is required."));

        if (string.IsNullOrWhiteSpace(statusCode))
            return Result.Failure<SignonSession>(new Error("SignonSession.InvalidStatusCode", "Status code is required."));

        var entity = new SignonSession
        {
            ImportId       = importId,
            StatusCode     = statusCode.Trim(),
            StatusSeverity = statusSeverity.Trim(),
            ServerDateRaw  = serverDateRaw.Trim(),
            ServerDate     = serverDate,
            Language       = language.Trim(),
            CreatedAt      = DateTime.UtcNow
        };

        return Result.Success(entity);
    }

    public Result Update(string statusCode, string statusSeverity, string language)
    {
        StatusCode     = statusCode.Trim();
        StatusSeverity = statusSeverity.Trim();
        Language       = language.Trim();
        SetUpdatedAt(DateTime.UtcNow);
        return Result.Success();
    }
}
