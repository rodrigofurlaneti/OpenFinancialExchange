using OpenFinancialExchange.Domain.Primitives;

namespace OpenFinancialExchange.Domain.Entities;

/// <summary>
/// Categoria de transação (ex: Alimentação, Transporte, Salário).
/// Categorias com <see cref="IsSystem"/> = true são semeadas e não podem ser
/// editadas ou removidas pelo usuário.
/// </summary>
public sealed class Category : AggregateRoot
{
    /// <summary>Dono da categoria. Null = categoria de sistema (compartilhada por todos).</summary>
    public long? UserId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Kind { get; private set; } = null!;
    public string Color { get; private set; } = null!;
    public bool IsSystem { get; private set; }

    /// <summary>
    /// Movimentação interna (transferências entre contas próprias, aplicação/resgate
    /// de investimento automático). Não conta como receita nem despesa no dashboard.
    /// </summary>
    public bool IsInternal { get; private set; }

    /// <summary>
    /// Palavras-chave (uma por linha) para auto-categorização: se a descrição da
    /// transação contiver alguma delas, recebe esta categoria.
    /// </summary>
    public string Keywords { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Category() : base(0) { }  // EF Core

    private Category(long? userId, string name, string kind, string color, bool isSystem, bool isInternal, string keywords) : base(0)
    {
        UserId = userId;
        Name = name;
        Kind = kind;
        Color = color;
        IsSystem = isSystem;
        IsInternal = isInternal;
        Keywords = keywords;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Restringe a quais transações a categoria se aplica.</summary>
    public static readonly HashSet<string> ValidKinds = ["CREDIT", "DEBIT", "BOTH"];

    public static Result<Category> Create(long? userId, string name, string kind, string color, bool isSystem = false, bool isInternal = false, string? keywords = null)
    {
        if (!isSystem && userId is null or <= 0)
            return Result.Failure<Category>(
                new Error("Category.InvalidUser", "A valid user is required for a custom category."));

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Category>(
                new Error("Category.EmptyName", "Category name is required."));

        if (name.Length > 50)
            return Result.Failure<Category>(
                new Error("Category.NameTooLong", "Category name must not exceed 50 characters."));

        if (!ValidKinds.Contains(kind?.ToUpperInvariant() ?? string.Empty))
            return Result.Failure<Category>(
                new Error("Category.InvalidKind", $"Kind must be one of: {string.Join(", ", ValidKinds)}."));

        var normalizedColor = NormalizeColor(color);
        if (normalizedColor is null)
            return Result.Failure<Category>(
                new Error("Category.InvalidColor", "Color must be a hex value like '#10b981'."));

        return Result.Success(new Category(isSystem ? null : userId, name.Trim(), kind!.ToUpperInvariant(), normalizedColor, isSystem, isInternal, keywords?.Trim() ?? string.Empty));
    }

    public Result Update(string name, string kind, string color, bool isInternal, string? keywords = null)
    {
        if (IsSystem)
            return Result.Failure(
                new Error("Category.SystemReadOnly", "System categories cannot be modified."));

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(new Error("Category.EmptyName", "Category name is required."));

        if (name.Length > 50)
            return Result.Failure(new Error("Category.NameTooLong", "Category name must not exceed 50 characters."));

        if (!ValidKinds.Contains(kind?.ToUpperInvariant() ?? string.Empty))
            return Result.Failure(new Error("Category.InvalidKind", $"Kind must be one of: {string.Join(", ", ValidKinds)}."));

        var normalizedColor = NormalizeColor(color);
        if (normalizedColor is null)
            return Result.Failure(new Error("Category.InvalidColor", "Color must be a hex value like '#10b981'."));

        Name = name.Trim();
        Kind = kind!.ToUpperInvariant();
        Color = normalizedColor;
        IsInternal = isInternal;
        Keywords = keywords?.Trim() ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (IsSystem)
            return Result.Failure(
                new Error("Category.SystemReadOnly", "System categories cannot be removed."));

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    private static string? NormalizeColor(string? color)
    {
        if (string.IsNullOrWhiteSpace(color)) return null;
        var c = color.Trim();
        if (c[0] != '#') c = "#" + c;
        if (c.Length != 7) return null;
        for (var i = 1; i < c.Length; i++)
        {
            var ch = char.ToLowerInvariant(c[i]);
            var isHex = ch is >= '0' and <= '9' or >= 'a' and <= 'f';
            if (!isHex) return null;
        }
        return c.ToLowerInvariant();
    }
}
