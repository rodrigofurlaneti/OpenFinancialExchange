using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Infrastructure.Persistence.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("OFX_Account", "ofx");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.ImportId)
            .HasColumnName("ImportId")
            .IsRequired();

        builder.Property(e => e.BankId)
            .HasColumnName("BankId")
            .IsRequired();

        builder.Property(e => e.BranchNumber)
            .HasColumnName("BranchNumber")
            .HasMaxLength(10);

        builder.Property(e => e.AccountNumber)
            .HasColumnName("AccountNumber")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(e => e.AccountType)
            .HasColumnName("AccountType")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.DefaultCurrency)
            .HasColumnName("DefaultCurrency")
            .HasMaxLength(3)
            .IsFixedLength()
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        // Account has no UpdatedAt column in the DB
        builder.Ignore(e => e.UpdatedAt);

        builder.HasOne<Import>()
            .WithMany()
            .HasForeignKey(e => e.ImportId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Bank>()
            .WithMany()
            .HasForeignKey(e => e.BankId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
