using OpenFinancialExchange.Domain.Primitives;

namespace OpenFinancialExchange.Domain.Entities;

public sealed class User : AggregateRoot
{
    private User() : base(0) { }  // EF Core

    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Result<User> Create(string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<User>(new Error("User.InvalidEmail", "E-mail não pode ser vazio."));

        if (string.IsNullOrWhiteSpace(passwordHash))
            return Result.Failure<User>(new Error("User.InvalidPassword", "Hash de senha inválido."));

        return Result.Success(new User
        {
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        });
    }
}
