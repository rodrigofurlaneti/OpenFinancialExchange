namespace OpenFinancialExchange.Domain.Common;

public sealed record Error(string Code, string Description)
{
    public static readonly Error None        = new(string.Empty, string.Empty);
    public static readonly Error NullValue   = new("General.Null",     "Value cannot be null.");
    public static readonly Error NotFound    = new("General.NotFound", "Resource not found.");
    public static readonly Error Conflict    = new("General.Conflict", "Resource already exists.");
    public static readonly Error Invalid     = new("General.Invalid",  "Invalid input.");
}
