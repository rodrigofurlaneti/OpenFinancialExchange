using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Infrastructure.Persistence.Configurations;

public sealed class LedgerBalanceConfiguration : IEntityTypeConfiguration<LedgerBalance>
{
    public void Configure(EntityTypeBuilder<LedgerBalance> builder)
    {
        builder.ToTable("OFX_LedgerBalance", "ofx");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.StatementId)
            .HasColumnName("StatementId")
            .IsRequired();

        builder.Property(e => e.BalanceType)
            .HasColumnName("BalanceType")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Amount)
            .HasColumnName("Amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.AsOfDate)
            .HasColumnName("AsOfDate")
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        // LedgerBalance has no UpdatedAt column in the DB
        builder.Ignore(e => e.UpdatedAt);

        builder.HasOne<Statement>()
            .WithMany()
            .HasForeignKey(e => e.StatementId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
