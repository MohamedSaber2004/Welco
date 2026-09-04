using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Welco.Shared.Domain.Models;
namespace Welco.Shared.Persistance.Configurations
{
    public class DocumentConfiguration : IEntityTypeConfiguration<Document>
    {
        public void Configure(EntityTypeBuilder<Document> b)
        {
            b.ToTable("Documents"); b.HasKey(x => x.Id);
            b.Property(x => x.Title).IsRequired().HasMaxLength(200);
            b.Property(x => x.DocType).IsRequired().HasMaxLength(50);
            b.Property(x => x.FileUrl).IsRequired().HasMaxLength(1000);
            b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.SetNull);
            b.Property(x => x.CreatedBy).IsRequired();
        }
    }
    public class LandingPageConfiguration : IEntityTypeConfiguration<LandingPage>
    {
        public void Configure(EntityTypeBuilder<LandingPage> b)
        {
            b.ToTable("LandingPages"); b.HasKey(x => x.Id);
            b.Property(x => x.Type).IsRequired().HasMaxLength(50);
            b.Property(x => x.Slug).IsRequired().HasMaxLength(200); b.HasIndex(x => x.Slug).IsUnique();
            b.Property(x => x.HeroTitle).IsRequired().HasMaxLength(300);
            b.Property(x => x.HeroBody).HasMaxLength(2000);
            b.Property(x => x.ContentBlock).HasMaxLength(4000);
            b.Property(x => x.CreatedBy).IsRequired();
        }
    }
    public class HelpCategoryConfiguration : IEntityTypeConfiguration<HelpCategory>
    {
        public void Configure(EntityTypeBuilder<HelpCategory> b)
        {
            b.ToTable("HelpCategories"); b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.Property(x => x.Icon).HasMaxLength(100);
            b.Property(x => x.CreatedBy).IsRequired();
        }
    }
    public class HelpArticleConfiguration : IEntityTypeConfiguration<HelpArticle>
    {
        public void Configure(EntityTypeBuilder<HelpArticle> b)
        {
            b.ToTable("HelpArticles"); b.HasKey(x => x.Id);
            b.Property(x => x.Title).IsRequired().HasMaxLength(300);
            b.Property(x => x.Body).IsRequired();
            b.Property(x => x.Slug).IsRequired().HasMaxLength(200); b.HasIndex(x => x.Slug).IsUnique();
            b.HasOne(x => x.Category).WithMany(x => x.Articles).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Cascade);
            b.Property(x => x.CreatedBy).IsRequired();
        }
    }
    public class FAQItemConfiguration : IEntityTypeConfiguration<FAQItem>
    {
        public void Configure(EntityTypeBuilder<FAQItem> b)
        {
            b.ToTable("FAQItems"); b.HasKey(x => x.Id);
            b.Property(x => x.Question).IsRequired().HasMaxLength(500);
            b.Property(x => x.Answer).IsRequired();
            b.Property(x => x.CreatedBy).IsRequired();
        }
    }
    public class TradeShowEventConfiguration : IEntityTypeConfiguration<TradeShowEvent>
    {
        public void Configure(EntityTypeBuilder<TradeShowEvent> b)
        {
            b.ToTable("TradeShowEvents"); b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.Property(x => x.Location).IsRequired().HasMaxLength(300);
            b.Property(x => x.CreatedBy).IsRequired();
        }
    }
    public class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
    {
        public void Configure(EntityTypeBuilder<BlogPost> b)
        {
            b.ToTable("BlogPosts"); b.HasKey(x => x.Id);
            b.Property(x => x.Title).IsRequired().HasMaxLength(300);
            b.Property(x => x.Body).IsRequired();
            b.Property(x => x.CreatedBy).IsRequired();
        }
    }
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> b)
        {
            b.ToTable("Notifications"); b.HasKey(x => x.Id);
            b.Property(x => x.Type).IsRequired().HasMaxLength(50);
            b.Property(x => x.Message).IsRequired().HasMaxLength(1000);
            b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.UserId, x.IsRead });
            b.Property(x => x.CreatedBy).IsRequired();
        }
    }
    public class SupportTicketConfiguration : IEntityTypeConfiguration<SupportTicket>
    {
        public void Configure(EntityTypeBuilder<SupportTicket> b)
        {
            b.ToTable("SupportTickets"); b.HasKey(x => x.Id);
            b.Property(x => x.Subject).IsRequired().HasMaxLength(300);
            b.Property(x => x.Message).IsRequired();
            b.Property(x => x.Status).IsRequired().HasMaxLength(20);
            b.Property(x => x.Reply).HasMaxLength(2000);
            b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => x.UserId);
            b.HasIndex(x => x.Status);
            b.Property(x => x.CreatedBy).IsRequired();
        }
    }
    public class SupportContactConfiguration : IEntityTypeConfiguration<SupportContact>
    {
        public void Configure(EntityTypeBuilder<SupportContact> b)
        {
            b.ToTable("SupportContacts");
            b.HasKey(x => x.Id);
            b.Property(x => x.SupportEmail).IsRequired().HasMaxLength(256);
            b.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(50);
            b.Property(x => x.WhatsAppNumber).IsRequired().HasMaxLength(50);
            b.Property(x => x.WorkingHours).HasMaxLength(200);
            b.Property(x => x.CreatedBy).IsRequired();
        }
    }
}
