using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Infrastructure.Persistence.Configurations;

public sealed class StatementConfiguration : IEntityTypeConfiguration<Statement>
{
    public void Configure(EntityTypeBuilder<Statement> builder)
    {
        builder.ToTable("OFX_Statement", "ofx");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.AccountId)
            .HasColumnName("AccountId")
            .IsRequired();

        builder.Property(e => e.TRNUID)
            .HasColumnName("TRNUID")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.StatusCode)
            .HasColumnName("StatusCode")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(e => e.StatusSeverity)
            .HasColumnName("StatusSeverity")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.StartDate)
            .HasColumnName("StartDate")
            .IsRequired();

        builder.Property(e => e.EndDate)
            .HasColumnName("EndDate")
            .IsRequired();

        builder.Property(e => e.TimeZone)
            .HasColumnName("TimeZone")
            .HasMaxLength(20);

        builder.Property(e => e.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        // Statement has no UpdatedAt column in the DB
        builder.Ignore(e => e.UpdatedAt);

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(e => e.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
