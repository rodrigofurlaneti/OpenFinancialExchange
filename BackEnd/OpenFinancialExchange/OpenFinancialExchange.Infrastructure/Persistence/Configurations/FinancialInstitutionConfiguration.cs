using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Infrastructure.Persistence.Configurations;

internal sealed class FinancialInstitutionConfiguration : IEntityTypeConfiguration<FinancialInstitution>
{
    public void Configure(EntityTypeBuilder<FinancialInstitution> builder)
    {
        builder.ToTable("FinancialInstitutions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("Id")
            .UseIdentityColumn();

        builder.Property(x => x.UserId)
            .HasColumnName("UserId")
            .IsRequired();

        builder.Property(x => x.BankId)
            .HasColumnName("BankId")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.OrgName)
            .HasColumnName("OrgName")
            .HasMaxLength(100);

        builder.Property(x => x.Fid)
            .HasColumnName("Fid")
            .HasMaxLength(50);

        builder.Property(x => x.IsActive)
            .HasColumnName("IsActive")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("CreatedAt")
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("UpdatedAt")
            .HasColumnType("datetime2")
            .IsRequired();

        builder.HasIndex(x => new { x.UserId, x.BankId, x.Fid })
            .IsUnique()
            .HasDatabaseName("UQ_FinancialInstitutions_User_BankId_Fid");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("FK_FinancialInstitutions_Users")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
