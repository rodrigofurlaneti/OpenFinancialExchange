using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Infrastructure.Persistence.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    // Timestamp fixo para o seed (HasData exige valores determinísticos).
    private static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder.Property(x => x.UserId).HasColumnName("UserId");  // nullable = categoria de sistema
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Kind).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Color).HasMaxLength(7).IsRequired();
        builder.Property(x => x.IsSystem).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnType("datetime2").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("datetime2").IsRequired();

        // Nome único por usuário; categorias de sistema (UserId null) são únicas entre si.
        builder.HasIndex(x => new { x.UserId, x.Name })
            .IsUnique()
            .HasDatabaseName("UX_Categories_User_Name");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("FK_Categories_Users")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(SeedCategories());
    }

    private static IEnumerable<object> SeedCategories()
    {
        // (Id, Nome, Kind, Cor). IDs fixos para o seed; o SQL Server avança a
        // identity automaticamente ao inserir valores explícitos via IDENTITY_INSERT.
        var defs = new (long Id, string Name, string Kind, string Color)[]
        {
            (1,  "Alimentação",        "DEBIT",  "#ef4444"),
            (2,  "Transporte",         "DEBIT",  "#f97316"),
            (3,  "Moradia",            "DEBIT",  "#eab308"),
            (4,  "Saúde",              "DEBIT",  "#14b8a6"),
            (5,  "Educação",           "DEBIT",  "#3b82f6"),
            (6,  "Lazer",              "DEBIT",  "#a855f7"),
            (7,  "Compras",            "DEBIT",  "#ec4899"),
            (8,  "Serviços",           "DEBIT",  "#64748b"),
            (9,  "Salário",            "CREDIT", "#22c55e"),
            (10, "Investimentos",      "BOTH",   "#10b981"),
            (11, "Transferências",     "BOTH",   "#06b6d4"),
            (12, "Tarifas Bancárias",  "DEBIT",  "#f43f5e"),
            (13, "Outros",             "BOTH",   "#94a3b8"),
        };

        foreach (var d in defs)
            yield return new
            {
                d.Id,
                UserId = (long?)null,   // categoria de sistema, compartilhada por todos
                d.Name,
                d.Kind,
                d.Color,
                IsSystem = true,
                IsActive = true,
                CreatedAt = SeedDate,
                UpdatedAt = SeedDate,
            };
    }
}
