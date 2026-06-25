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

        builder.Property(x => x.UserId).HasColumnName("UserId").IsRequired();
        builder.Property(x => x.StatementId).IsRequired();
        builder.Property(x => x.TrnType).HasMaxLength(20).IsRequired();
        builder.Property(x => x.DtPosted).HasColumnType("datetime2").IsRequired();
        builder.Property(x => x.TrnAmt).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.FitId).HasMaxLength(255);
        builder.Property(x => x.Name).HasMaxLength(100);
        builder.Property(x => x.Memo).HasMaxLength(255);
        builder.Property(x => x.CheckNum).HasMaxLength(20);
        builder.Property(x => x.CategoryId);
        builder.Property(x => x.CreatedAt).HasColumnType("datetime2").IsRequired();

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .HasConstraintName("FK_OfxTransactions_Categories")
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("FK_OfxTransactions_Users")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.CategoryId)
            .HasDatabaseName("IX_OfxTransactions_CategoryId");

        builder.HasIndex(x => new { x.StatementId, x.FitId })
            .IsUnique()
            .HasFilter("[FitId] IS NOT NULL")
            .HasDatabaseName("UIX_OfxTransactions_StatementId_FitId");

        builder.HasIndex(x => new { x.UserId, x.DtPosted, x.TrnType })
            .HasDatabaseName("IX_OfxTransactions_User_DtPosted_TrnType");

        // FK configured from the OfxStatement side via HasMany + UsePropertyAccessMode(Field)
    }
}
