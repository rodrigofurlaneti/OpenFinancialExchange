using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Infrastructure.Persistence.Configurations;

public sealed class TransactionCategoryConfiguration
    : IEntityTypeConfiguration<TransactionCategory>
{
    public void Configure(EntityTypeBuilder<TransactionCategory> builder)
    {
        builder.ToTable("OFX_TransactionCategory", "ofx");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Code)
            .HasColumnName("Code")
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasColumnName("Description")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.OperationType)
            .HasColumnName("OperationType")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(e => e.AccountingNature)
            .HasColumnName("AccountingNature")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.IsActive)
            .HasColumnName("IsActive")
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("UpdatedAt");

        builder.HasIndex(e => e.Code)
            .IsUnique()
            .HasDatabaseName("UX_TransactionCategory_Code");
    }
}
