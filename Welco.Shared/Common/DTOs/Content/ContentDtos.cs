namespace Welco.Shared.Common.DTOs.Content
{
    public class DocumentDto { public Guid Id { get; set; } public string Title { get; set; } = string.Empty; public string DocType { get; set; } = string.Empty; public string FileUrl { get; set; } = string.Empty; public int FileSizeKB { get; set; } public Guid? ProductId { get; set; } public DateTime PublishedDate { get; set; } public DateTime CreatedAt { get; set; } }
    public class LandingPageDto { public Guid Id { get; set; } public string Type { get; set; } = string.Empty; public string Slug { get; set; } = string.Empty; public string HeroTitle { get; set; } = string.Empty; public DateTime CreatedAt { get; set; } }
}
