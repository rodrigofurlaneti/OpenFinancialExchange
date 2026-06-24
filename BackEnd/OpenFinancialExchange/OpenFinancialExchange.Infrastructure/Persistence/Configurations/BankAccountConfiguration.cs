using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Infrastructure.Persistence.Configurations;

internal sealed class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> builder)
    {
        builder.ToTable("BankAccounts");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder.Property(x => x.FinancialInstitutionId).HasColumnName("FinancialInstitutionId").IsRequired();
        builder.Property(x => x.BankId).HasColumnName("BankId").HasMaxLength(20).IsRequired();
        builder.Property(x => x.BranchId).HasColumnName("BranchId").HasMaxLength(20);
        builder.Property(x => x.AcctId).HasColumnName("AcctId").HasMaxLength(50).IsRequired();
        builder.Property(x => x.AcctType).HasColumnName("AcctType").HasMaxLength(20).IsRequired();
        builder.Property(x => x.IsActive).HasColumnName("IsActive").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("CreatedAt").HasColumnType("datetime2").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("UpdatedAt").HasColumnType("datetime2").IsRequired();

        builder.HasIndex(x => new { x.BankId, x.BranchId, x.AcctId })
            .IsUnique()
            .HasDatabaseName("UQ_BankAccounts_BankId_BranchId_AcctId");

        builder.HasOne<FinancialInstitution>()
            .WithMany()
            .HasForeignKey(x => x.FinancialInstitutionId)
            .HasConstraintName("FK_BankAccounts_FinancialInstitutions")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
