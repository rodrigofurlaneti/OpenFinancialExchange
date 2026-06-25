using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Infrastructure.Persistence.Configurations;

internal sealed class OfxBalanceConfiguration : IEntityTypeConfiguration<OfxBalance>
{
    public void Configure(EntityTypeBuilder<OfxBalance> builder)
    {
        builder.ToTable("OfxBalances");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder.Property(x => x.UserId).HasColumnName("UserId").IsRequired();
        builder.Property(x => x.StatementId).IsRequired();
        builder.Property(x => x.BalanceType).HasMaxLength(20).IsRequired();
        builder.Property(x => x.BalAmt).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.DtAsOf).HasColumnType("datetime2").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnType("datetime2").IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("FK_OfxBalances_Users")
            .OnDelete(DeleteBehavior.Restrict);

        // Statement FK configured from the OfxStatement side via HasMany + UsePropertyAccessMode(Field)
    }
}
