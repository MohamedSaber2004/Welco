using System.Linq.Expressions;
using Welco.Shared.Common.DTOs.Content;
using DocumentEntity = Welco.Shared.Domain.Models.Document;
using LandingPageEntity = Welco.Shared.Domain.Models.LandingPage;

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
    }
}
