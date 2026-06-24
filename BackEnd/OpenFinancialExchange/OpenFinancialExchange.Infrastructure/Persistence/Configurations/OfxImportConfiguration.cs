using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Infrastructure.Persistence.Configurations;

internal sealed class OfxImportConfiguration : IEntityTypeConfiguration<OfxImport>
{
    public void Configure(EntityTypeBuilder<OfxImport> builder)
    {
        builder.ToTable("OfxImports");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder.Property(x => x.FileName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.FileHash).HasMaxLength(64).IsRequired();
        builder.Property(x => x.OfxHeaderVersion).HasColumnName("OfxHeaderVersion");
        builder.Property(x => x.OfxVersion).HasColumnName("OfxVersion");
        builder.Property(x => x.OfxData).HasColumnName("OfxData").HasColumnType("nvarchar(max)");
        builder.Property(x => x.Encoding).HasMaxLength(20);
        builder.Property(x => x.Charset).HasMaxLength(20);
        builder.Property(x => x.Security).HasMaxLength(20);
        builder.Property(x => x.Compression).HasMaxLength(20);
        builder.Property(x => x.OldFileUid).HasMaxLength(50);
        builder.Property(x => x.NewFileUid).HasMaxLength(50);
        builder.Property(x => x.ImportedAt).HasColumnType("datetime2").IsRequired();

        builder.HasIndex(x => x.FileHash)
            .IsUnique()
            .HasDatabaseName("UQ_OfxImports_FileHash");
    }
}
