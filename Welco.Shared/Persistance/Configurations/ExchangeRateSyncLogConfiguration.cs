using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Welco.Shared.Domain.Models;

namespace Welco.Shared.Persistance.Configurations
{
    public class ExchangeRateSyncLogConfiguration : IEntityTypeConfiguration<ExchangeRateSyncLog>
    {
        public void Configure(EntityTypeBuilder<ExchangeRateSyncLog> builder)
        {
            builder.ToTable("ExchangeRateSyncLogs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.StartedAt).IsRequired();
            builder.Property(x => x.CompletedAt);
            builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.BaseCurrency).IsRequired().HasMaxLength(10);
            builder.Property(x => x.RatesCount).IsRequired();
            builder.Property(x => x.Source).IsRequired().HasMaxLength(100);
            builder.Property(x => x.ErrorMessage).HasMaxLength(2000);
        }
    }
}
