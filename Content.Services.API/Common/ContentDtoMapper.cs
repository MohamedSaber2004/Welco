using System.Linq.Expressions;
using Welco.Shared.Common.DTOs.Content;
using DocumentEntity = Welco.Shared.Domain.Models.Document;
using LandingPageEntity = Welco.Shared.Domain.Models.LandingPage;
using HelpCategoryEntity = Welco.Shared.Domain.Models.HelpCategory;
using HelpArticleEntity = Welco.Shared.Domain.Models.HelpArticle;
using FAQEntity = Welco.Shared.Domain.Models.FAQItem;
using SupportTicketEntity = Welco.Shared.Domain.Models.SupportTicket;
using SupportContactEntity = Welco.Shared.Domain.Models.SupportContact;

namespace Content.Services.API.Common
{
    internal static class ContentDtoMapper
    {
        public static Expression<Func<DocumentEntity, DocumentDto>> DocumentProjection => d => new DocumentDto
        {
            Id = d.Id,
            Title = d.Title,
            DocType = d.DocType,
            FileUrl = d.FileUrl,
            FileSizeKB = d.FileSizeKB,
            ProductId = d.ProductId,
            PublishedDate = d.PublishedDate,
            CreatedAt = d.CreatedAt
        };

        public static Expression<Func<LandingPageEntity, LandingPageDto>> LandingPageProjection => l => new LandingPageDto
        {
            Id = l.Id,
            Type = l.Type,
            Slug = l.Slug,
            HeroTitle = l.HeroTitle,
            CreatedAt = l.CreatedAt
        };

        public static Expression<Func<HelpCategoryEntity, HelpCategoryDto>> HelpCategoryProjection => c => new HelpCategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Icon = c.Icon,
            ArticleCount = c.Articles.Count(a => !a.IsDeleted),
            CreatedAt = c.CreatedAt
        };

        public static Expression<Func<HelpArticleEntity, HelpArticleDto>> HelpArticleProjection => a => new HelpArticleDto
        {
            Id = a.Id,
            CategoryId = a.CategoryId,
            CategoryName = a.Category != null ? a.Category.Name : string.Empty,
            Title = a.Title,
            Body = a.Body,
            Slug = a.Slug,
            CreatedAt = a.CreatedAt
        };

        public static Expression<Func<FAQEntity, FAQItemDto>> FAQProjection => f => new FAQItemDto
        {
            Id = f.Id,
            Question = f.Question,
            Answer = f.Answer,
            SortOrder = f.SortOrder,
            CreatedAt = f.CreatedAt
        };

        public static Expression<Func<SupportTicketEntity, SupportTicketDto>> SupportTicketProjection => t => new SupportTicketDto
        {
            Id = t.Id,
            UserId = t.UserId,
            Subject = t.Subject,
            Message = t.Message,
            Status = t.Status,
            Reply = t.Reply,
            CreatedAt = t.CreatedAt,
            RepliedAt = t.RepliedAt
        };

        public static Expression<Func<SupportContactEntity, SupportContactDto>> SupportContactProjection => c => new SupportContactDto
        {
            Id = c.Id,
            SupportEmail = c.SupportEmail,
            PhoneNumber = c.PhoneNumber,
            WhatsAppNumber = c.WhatsAppNumber,
            WorkingHours = c.WorkingHours,
            UpdatedAt = c.UpdatedAt ?? c.CreatedAt
        };
    }
}
