using CleanArchitecture.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Infrastructure.Data.Configurations;

public class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.Property(e => e.Action)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(e => e.TargetType)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(e => e.ActorName)
            .HasMaxLength(256);

        builder.Property(e => e.TargetName)
            .HasMaxLength(256);

        builder.HasIndex(e => e.Timestamp);
    }
}
