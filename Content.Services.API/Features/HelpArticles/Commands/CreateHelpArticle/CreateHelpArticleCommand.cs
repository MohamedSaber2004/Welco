using MediatR;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Results;

namespace Content.Services.API.Features.HelpArticles.Commands.CreateHelpArticle
{
    public class CreateHelpArticleCommand : IRequest<Result<HelpArticleDto>>
    {
        public Guid CategoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
    }
}
