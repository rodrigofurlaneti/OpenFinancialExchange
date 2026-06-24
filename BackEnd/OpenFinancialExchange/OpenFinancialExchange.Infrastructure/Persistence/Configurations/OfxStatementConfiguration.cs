using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Infrastructure.Persistence.Configurations;

internal sealed class OfxStatementConfiguration : IEntityTypeConfiguration<OfxStatement>
{
    public void Configure(EntityTypeBuilder<OfxStatement> builder)
    {
        builder.ToTable("OfxStatements");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder.Property(x => x.ImportId).IsRequired();
        builder.Property(x => x.BankAccountId).IsRequired();
        builder.Property(x => x.TrnUid).HasMaxLength(36);
        builder.Property(x => x.CurDef).HasMaxLength(3).IsRequired();
        builder.Property(x => x.DtServer).HasColumnType("datetime2");
        builder.Property(x => x.Language).HasMaxLength(10);
        builder.Property(x => x.StatusCode);
        builder.Property(x => x.StatusSeverity).HasMaxLength(10);
        builder.Property(x => x.DtStart).HasColumnType("datetime2");
        builder.Property(x => x.DtEnd).HasColumnType("datetime2");
        builder.Property(x => x.CreatedAt).HasColumnType("datetime2").IsRequired();

        builder.HasOne<OfxImport>()
            .WithMany()
            .HasForeignKey(x => x.ImportId)
            .HasConstraintName("FK_OfxStatements_OfxImports")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<BankAccount>()
            .WithMany()
            .HasForeignKey(x => x.BankAccountId)
            .HasConstraintName("FK_OfxStatements_BankAccounts")
            .OnDelete(DeleteBehavior.Restrict);

        // Private backing-field collections: EF Core populates via field access
        builder.HasMany(x => x.Transactions)
            .WithOne()
            .HasForeignKey(nameof(OfxTransaction.StatementId))
            .HasConstraintName("FK_OfxTransactions_OfxStatements")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Transactions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Balances)
            .WithOne()
            .HasForeignKey(nameof(OfxBalance.StatementId))
            .HasConstraintName("FK_OfxBalances_OfxStatements")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Balances)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
