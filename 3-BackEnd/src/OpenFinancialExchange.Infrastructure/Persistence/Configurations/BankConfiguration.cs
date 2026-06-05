using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Infrastructure.Persistence.Configurations;

public sealed class BankConfiguration : IEntityTypeConfiguration<Bank>
{
    public void Configure(EntityTypeBuilder<Bank> builder)
    {
        builder.ToTable("OFX_Bank", "ofx");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.COMPECode)
            .HasColumnName("COMPECode")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(e => e.BankName)
            .HasColumnName("BankName")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(e => e.ISPB)
            .HasColumnName("ISPB")
            .HasMaxLength(20);

        builder.Property(e => e.IsActive)
            .HasColumnName("IsActive")
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("UpdatedAt");

        builder.HasIndex(e => e.COMPECode)
            .IsUnique()
            .HasDatabaseName("UX_Bank_COMPECode");
    }
}
