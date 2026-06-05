using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Infrastructure.Persistence.Configurations;

public sealed class ImportConfiguration : IEntityTypeConfiguration<Import>
{
    public void Configure(EntityTypeBuilder<Import> builder)
    {
        builder.ToTable("OFX_Import", "ofx");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.FileName)
            .HasColumnName("FileName")
            .HasMaxLength(260)
            .IsRequired();

        builder.Property(e => e.ImportedAt)
            .HasColumnName("ImportedAt")
            .IsRequired();

        builder.Property(e => e.OFXHeader)
            .HasColumnName("OFXHeader")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(e => e.OFXData)
            .HasColumnName("OFXData")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.OFXVersion)
            .HasColumnName("OFXVersion")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(e => e.OFXSecurity)
            .HasColumnName("OFXSecurity")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.OFXEncoding)
            .HasColumnName("OFXEncoding")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.OFXCharset)
            .HasColumnName("OFXCharset")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(e => e.OFXCompression)
            .HasColumnName("OFXCompression")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.OFXOldFileUID)
            .HasColumnName("OFXOldFileUID")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.OFXNewFileUID)
            .HasColumnName("OFXNewFileUID")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Notes)
            .HasColumnName("Notes")
            .HasMaxLength(500);

        builder.Property(e => e.ImportedBy)
            .HasColumnName("ImportedBy")
            .HasMaxLength(100);

        builder.Property(e => e.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        // Import has no UpdatedAt column in the DB
        builder.Ignore(e => e.UpdatedAt);
    }
}
