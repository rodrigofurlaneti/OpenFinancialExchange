using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Infrastructure.Persistence.Configurations;

internal sealed class BankCodeConfiguration : IEntityTypeConfiguration<BankCode>
{
    public void Configure(EntityTypeBuilder<BankCode> builder)
    {
        builder.ToTable("BankCodes");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .ValueGeneratedNever(); // PK manual = CodigoCompensacao

        builder.Property(b => b.CodigoCompensacao)
            .IsRequired();

        builder.Property(b => b.Cnpj)
            .HasMaxLength(14)
            .IsRequired();

        builder.Property(b => b.NomeInstituicao)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(b => b.Segmento)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(b => b.CodigoCompensacao)
            .IsUnique()
            .HasDatabaseName("UX_BankCodes_CodigoCompensacao");
    }
}
