using Welco.Shared.Common.Classes;
namespace Welco.Shared.Domain.Models
{
    public class Document : BaseEntity<Guid>
    {
        public string Title { get; set; } = null!;
        public string DocType { get; set; } = null!; // Catalog, Brochure, IFU, Certificate
        public string FileUrl { get; set; } = null!;
        public int FileSizeKB { get; set; }
        public Guid? ProductId { get; set; }
        public virtual Product? Product { get; set; }
        public DateTime PublishedDate { get; set; }
    }
    public class LandingPage : BaseEntity<Guid>
    {
        public string Type { get; set; } = null!; // Brand, Specialty, Procedure
        public string Slug { get; set; } = null!;
        public string HeroTitle { get; set; } = null!;
        public string? HeroBody { get; set; }
        public string? ContentBlock { get; set; }
    }
    public class HelpCategory : BaseEntity<Guid>
    {
        public string Name { get; set; } = null!;
        public string? Icon { get; set; }
        public virtual ICollection<HelpArticle> Articles { get; set; } = new List<HelpArticle>();
    }
    public class HelpArticle : BaseEntity<Guid>
    {
        public Guid CategoryId { get; set; }
        public virtual HelpCategory? Category { get; set; }
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public string Slug { get; set; } = null!;
    }
    public class FAQItem : BaseEntity<Guid>
    {
        public string Question { get; set; } = null!;
        public string Answer { get; set; } = null!;
        public int SortOrder { get; set; }
    }
    public class TradeShowEvent : BaseEntity<Guid>
    {
        public string Name { get; set; } = null!;
        public string Location { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    public class BlogPost : BaseEntity<Guid>
    {
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public DateTime PublishedDate { get; set; }
    }
    public class Notification : BaseEntity<Guid>
    {
        public Guid UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public string Type { get; set; } = null!;
        public string Message { get; set; } = null!;
        public bool IsRead { get; set; }
    }
    public class SupportTicket : BaseEntity<Guid>
    {
        public Guid UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public string Subject { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string Status { get; set; } = "Open"; // Open, Answered, Closed
        public string? Reply { get; set; }
        public DateTime? RepliedAt { get; set; }
        public Guid? RepliedBy { get; set; }
    }
}
