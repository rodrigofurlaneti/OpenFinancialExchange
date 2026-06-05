using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Infrastructure.Persistence.Configurations;

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("OFX_Transaction", "ofx");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.StatementId)
            .HasColumnName("StatementId")
            .IsRequired();

        builder.Property(e => e.CategoryId)
            .HasColumnName("CategoryId");

        builder.Property(e => e.TransactionType)
            .HasColumnName("TransactionType")
            .HasMaxLength(15)
            .IsRequired();

        builder.Property(e => e.PostedDateRaw)
            .HasColumnName("PostedDateRaw")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(e => e.PostedDate)
            .HasColumnName("PostedDate")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(e => e.TimeZone)
            .HasColumnName("TimeZone")
            .HasMaxLength(20);

        builder.Property(e => e.Amount)
            .HasColumnName("Amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.FITID)
            .HasColumnName("FITID")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.CheckNumber)
            .HasColumnName("CheckNumber")
            .HasMaxLength(50);

        builder.Property(e => e.Memo)
            .HasColumnName("Memo")
            .HasMaxLength(500);

        // Computed persisted columns — managed by SQL Server, not by EF insert/update
        builder.Property(e => e.AbsoluteAmount)
            .HasColumnName("AbsoluteAmount")
            .HasColumnType("decimal(18,2)")
            .HasComputedColumnSql("ABS([Amount])", stored: true)
            .ValueGeneratedOnAddOrUpdate();

        builder.Property(e => e.MovementNature)
            .HasColumnName("MovementNature")
            .HasMaxLength(10)
            .HasComputedColumnSql("CASE WHEN [Amount] >= 0 THEN 'CREDIT' ELSE 'DEBIT' END", stored: true)
            .ValueGeneratedOnAddOrUpdate();

        builder.Property(e => e.PayeeName)
            .HasColumnName("PayeeName")
            .HasMaxLength(255);

        builder.Property(e => e.TransactionDateMemo)
            .HasColumnName("TransactionDateMemo")
            .HasMaxLength(10);

        builder.Property(e => e.OperationSubtype)
            .HasColumnName("OperationSubtype")
            .HasMaxLength(100);

        builder.Property(e => e.IsReconciled)
            .HasColumnName("IsReconciled")
            .IsRequired();

        builder.Property(e => e.ReconciledAt)
            .HasColumnName("ReconciledAt");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("UpdatedAt");

        builder.HasIndex(e => e.FITID)
            .IsUnique()
            .HasDatabaseName("UX_Transaction_FITID");

        builder.HasOne<Statement>()
            .WithMany()
            .HasForeignKey(e => e.StatementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<TransactionCategory>()
            .WithMany()
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}
