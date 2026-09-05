using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Welco.Shared.Domain.Models;
namespace Welco.Shared.Persistance.Configurations
{
    public class RFQConfiguration : IEntityTypeConfiguration<RFQ>
    {
        public void Configure(EntityTypeBuilder<RFQ> b)
        {
            b.ToTable("RFQs"); b.HasKey(x => x.Id);
            b.Property(x => x.RFQNumber).IsRequired().HasMaxLength(30); b.HasIndex(x => x.RFQNumber).IsUnique();
            b.Property(x => x.Status).HasConversion<string>().IsRequired();
            b.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.AssignedSalesRep).WithMany().HasForeignKey(x => x.AssignedSalesRepId).OnDelete(DeleteBehavior.SetNull);
            b.Property(x => x.CreatedBy).IsRequired();
        }
    }
    public class RFQItemConfiguration : IEntityTypeConfiguration<RFQItem>
    {
        public void Configure(EntityTypeBuilder<RFQItem> b)
        {
            b.ToTable("RFQItems"); b.HasKey(x => x.Id);
            b.HasOne(x => x.RFQ).WithMany(x => x.Items).HasForeignKey(x => x.RFQId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            b.Property(x => x.UnitPrice).HasPrecision(18,2);
            b.Property(x => x.Notes).HasMaxLength(500);
            b.Property(x => x.CreatedBy).IsRequired();
        }
    }
    public class QuoteConfiguration : IEntityTypeConfiguration<Quote>
    {
        public void Configure(EntityTypeBuilder<Quote> b)
        {
            b.ToTable("Quotes"); b.HasKey(x => x.Id);
            b.Property(x => x.QuoteNumber).IsRequired().HasMaxLength(30); b.HasIndex(x => x.QuoteNumber).IsUnique();
            b.Property(x => x.Status).HasConversion<string>().IsRequired();
            b.Property(x => x.Amount).HasPrecision(18,2);
            b.HasOne(x => x.RFQ).WithMany().HasForeignKey(x => x.RFQId).OnDelete(DeleteBehavior.SetNull);
            b.Property(x => x.CreatedBy).IsRequired();
        }
    }
    public class QuoteItemConfiguration : IEntityTypeConfiguration<QuoteItem>
    {
        public void Configure(EntityTypeBuilder<QuoteItem> b)
        {
            b.ToTable("QuoteItems"); b.HasKey(x => x.Id);
            b.HasOne(x => x.Quote).WithMany(x => x.Items).HasForeignKey(x => x.QuoteId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            b.Property(x => x.UnitPrice).HasPrecision(18,2);
            b.Property(x => x.CreatedBy).IsRequired();
        }
    }
    public class ProductInquiryConfiguration : IEntityTypeConfiguration<ProductInquiry>
    {
        public void Configure(EntityTypeBuilder<ProductInquiry> b)
        {
            b.ToTable("ProductInquiries"); b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.Property(x => x.Organization).IsRequired().HasMaxLength(200);
            b.Property(x => x.Message).IsRequired().HasMaxLength(2000);
            b.Property(x => x.Email).HasMaxLength(200);
            b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
            b.Property(x => x.CreatedBy).IsRequired();
        }
    }
    public class DistributorApplicationConfiguration : IEntityTypeConfiguration<DistributorApplication>
    {
        public void Configure(EntityTypeBuilder<DistributorApplication> b)
        {
            b.ToTable("DistributorApplications"); b.HasKey(x => x.Id);
            b.Property(x => x.CompanyName).IsRequired().HasMaxLength(200);
            b.Property(x => x.SalesVolumeBand).IsRequired().HasMaxLength(50);
            b.Property(x => x.Website).HasMaxLength(300);
            b.Property(x => x.ContactPerson).IsRequired().HasMaxLength(200);
            b.Property(x => x.ContactEmail).IsRequired().HasMaxLength(200);
            b.Property(x => x.Status).HasConversion<string>().IsRequired();
            b.HasOne(x => x.Country).WithMany().HasForeignKey(x => x.CountryId).OnDelete(DeleteBehavior.Restrict);
            b.Property(x => x.CreatedBy).IsRequired();
        }
    }
}
