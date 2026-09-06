using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Welco.Shared.Domain.Models;

namespace Welco.Shared.Persistance.Configurations
{
    public class ExchangeRateConfiguration : IEntityTypeConfiguration<Welco.Shared.Domain.Models.ExchangeRate>
    {
        public void Configure(EntityTypeBuilder<Welco.Shared.Domain.Models.ExchangeRate> builder)
        {
            builder.ToTable("ExchangeRates");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Rate)
                .IsRequired()
                .HasPrecision(28, 12);

            builder.Property(x => x.RateDate)
                .IsRequired()
                .HasConversion(
                    d => d.ToDateTime(TimeOnly.MinValue),
                    d => DateOnly.FromDateTime(d));

            builder.Property(x => x.Source)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.FetchedAt)
                .IsRequired();

            builder.HasOne(x => x.BaseCurrency)
                .WithMany()
                .HasForeignKey(x => x.BaseCurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.TargetCurrency)
                .WithMany()
                .HasForeignKey(x => x.TargetCurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.BaseCurrencyId, x.TargetCurrencyId, x.RateDate })
                .IsUnique();

            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_ExchangeRate_Rate_Positive", "[Rate] > 0");
                t.HasCheckConstraint("CK_ExchangeRate_Currencies_Different", "[BaseCurrencyId] <> [TargetCurrencyId]");
            });
        }
    }
}
