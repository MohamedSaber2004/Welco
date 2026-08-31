using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Welco.Shared.Domain.Models;

namespace Welco.Shared.Persistance.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.EntityName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.EntityId)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Action)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Details)
                .HasMaxLength(4000);

            builder.Property(x => x.PerformedBy)
                .HasMaxLength(100);

            builder.HasIndex(x => x.EntityName);
            builder.HasIndex(x => x.EntityId);
            builder.HasIndex(x => x.CreatedAt);

            builder.Property(x => x.CreatedBy)
                .IsRequired();
        }
    }
}
