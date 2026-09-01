using MediatR;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Results;

namespace Content.Services.API.Features.HelpArticles.Queries.GetHelpArticleBySlug
{
    public class GetHelpArticleBySlugQuery : IRequest<Result<HelpArticleDto>>
    {
        public string Slug { get; set; } = string.Empty;
    }
}
