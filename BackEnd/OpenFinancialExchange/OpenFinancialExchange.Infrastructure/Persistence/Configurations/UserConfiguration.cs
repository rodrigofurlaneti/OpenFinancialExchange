using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenFinancialExchange.Domain.Entities;

namespace OpenFinancialExchange.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder.Property(x => x.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(x => x.Email)
            .IsUnique()
            .HasDatabaseName("UX_Users_Email");

        builder.Property(x => x.PasswordHash)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();
    }
}
