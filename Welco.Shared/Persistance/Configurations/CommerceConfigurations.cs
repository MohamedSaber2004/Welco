using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Welco.Shared.Domain.Models;
namespace Welco.Shared.Persistance.Configurations
{
    public class CartConfiguration : IEntityTypeConfiguration<Cart>
    {
        public void Configure(EntityTypeBuilder<Cart> b)
        {
            b.ToTable("Carts");
            b.HasKey(x => x.Id);
            b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
            b.HasOne(x => x.Currency).WithMany().HasForeignKey(x => x.CurrencyId).OnDelete(DeleteBehavior.SetNull);
            b.HasIndex(x => x.UserId);
            b.HasIndex(x => x.SessionId);
            b.Property(x => x.CreatedBy).IsRequired();
        }
    }
    public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> b)
        {
            b.ToTable("CartItems");
            b.HasKey(x => x.Id);
            b.HasOne(x => x.Cart).WithMany(x => x.Items).HasForeignKey(x => x.CartId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            b.Property(x => x.UnitPriceSnapshot).HasPrecision(18,2);
            b.HasIndex(x => x.CartId);
            b.Property(x => x.CreatedBy).IsRequired();
        }
    }
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> b)
        {
            b.ToTable("Orders");
            b.HasKey(x => x.Id);
            b.Property(x => x.OrderNumber).IsRequired().HasMaxLength(30);
            b.HasIndex(x => x.OrderNumber).IsUnique();
            b.Property(x => x.Status).HasConversion<string>().IsRequired();
            b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
            b.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.SetNull);
            b.HasOne(x => x.Currency).WithMany().HasForeignKey(x => x.CurrencyId).OnDelete(DeleteBehavior.SetNull);
            b.Property(x => x.TotalAmount).HasPrecision(18,2);
            b.Property(x => x.CreatedBy).IsRequired();
        }
    }
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> b)
        {
            b.ToTable("OrderItems");
            b.HasKey(x => x.Id);
            b.HasOne(x => x.Order).WithMany(x => x.Items).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            b.Property(x => x.UnitPrice).HasPrecision(18,2);
            b.HasIndex(x => x.OrderId);
            b.Property(x => x.CreatedBy).IsRequired();
        }
    }
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> b)
        {
            b.ToTable("Invoices");
            b.HasKey(x => x.Id);
            b.Property(x => x.InvoiceNumber).IsRequired().HasMaxLength(30);
            b.HasIndex(x => x.InvoiceNumber).IsUnique();
            b.Property(x => x.Status).HasConversion<string>().IsRequired();
            b.HasOne(x => x.Order).WithMany(x => x.Invoices).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
            b.Property(x => x.Amount).HasPrecision(18,2);
            b.Property(x => x.FileUrl).HasMaxLength(1000);
            b.Property(x => x.CreatedBy).IsRequired();
        }
    }
}
