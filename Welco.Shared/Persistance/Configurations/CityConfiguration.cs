using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Welco.Shared.Domain.Models;

namespace Welco.Shared.Persistance.Configurations
{
    public class CityConfiguration : IEntityTypeConfiguration<City>
    {
        public void Configure(EntityTypeBuilder<City> builder)
        {
            builder.ToTable("Cities");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.NameEn)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.NameAr)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.CreatedBy)
                .IsRequired();

            builder.HasOne(x => x.Country)
                .WithMany(c => c.Cities)
                .HasForeignKey(x => x.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Zones)
                .WithOne(z => z.City)
                .HasForeignKey(z => z.CityId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
