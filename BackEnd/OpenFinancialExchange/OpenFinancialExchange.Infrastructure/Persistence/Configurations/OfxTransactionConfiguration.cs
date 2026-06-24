using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Infrastructure.Persistence.Configurations;

internal sealed class OfxTransactionConfiguration : IEntityTypeConfiguration<OfxTransaction>
{
    public void Configure(EntityTypeBuilder<OfxTransaction> builder)
    {
        builder.ToTable("OfxTransactions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder.Property(x => x.StatementId).IsRequired();
        builder.Property(x => x.TrnType).HasMaxLength(20).IsRequired();
        builder.Property(x => x.DtPosted).HasColumnType("datetime2").IsRequired();
        builder.Property(x => x.TrnAmt).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.FitId).HasMaxLength(255);
        builder.Property(x => x.Name).HasMaxLength(100);
        builder.Property(x => x.Memo).HasMaxLength(255);
        builder.Property(x => x.CheckNum).HasMaxLength(20);
        builder.Property(x => x.CreatedAt).HasColumnType("datetime2").IsRequired();

        builder.HasIndex(x => new { x.StatementId, x.FitId })
            .IsUnique()
            .HasFilter("[FitId] IS NOT NULL")
            .HasDatabaseName("UIX_OfxTransactions_StatementId_FitId");

        builder.HasIndex(x => new { x.DtPosted, x.TrnType })
            .HasDatabaseName("IX_OfxTransactions_DtPosted_TrnType");

        // FK configured from the OfxStatement side via HasMany + UsePropertyAccessMode(Field)
    }
}
