using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Welco.Shared.Domain.Models;

namespace Welco.Shared.Persistance.Configurations
{
    public class CertificationConfiguration : IEntityTypeConfiguration<Certification>
    {
        public void Configure(EntityTypeBuilder<Certification> builder)
        {
            builder.ToTable("Certifications");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.CertificateNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(x => x.CertificateNumber)
                .IsUnique();

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.IssuedTo)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Issuer)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.Property(x => x.CertificationImageName)
                .HasMaxLength(500);

            builder.Property(x => x.CreatedBy)
                .IsRequired();
        }
    }
}
