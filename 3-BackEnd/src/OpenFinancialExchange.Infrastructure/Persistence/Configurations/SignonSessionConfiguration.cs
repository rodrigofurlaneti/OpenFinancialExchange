using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Infrastructure.Persistence.Configurations;

public sealed class SignonSessionConfiguration : IEntityTypeConfiguration<SignonSession>
{
    public void Configure(EntityTypeBuilder<SignonSession> builder)
    {
        builder.ToTable("OFX_SignonSession", "ofx");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.ImportId)
            .HasColumnName("ImportId")
            .IsRequired();

        builder.Property(e => e.StatusCode)
            .HasColumnName("StatusCode")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(e => e.StatusSeverity)
            .HasColumnName("StatusSeverity")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.ServerDateRaw)
            .HasColumnName("ServerDateRaw")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.ServerDate)
            .HasColumnName("ServerDate");

        builder.Property(e => e.Language)
            .HasColumnName("Language")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        // SignonSession has no UpdatedAt column in the DB
        builder.Ignore(e => e.UpdatedAt);

        builder.HasOne<Import>()
            .WithMany()
            .HasForeignKey(e => e.ImportId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
