using MediatR;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Results;

namespace Content.Services.API.Features.HelpArticles.Queries.GetHelpArticles
{
    public class GetHelpArticlesQuery : IRequest<Result<List<HelpArticleDto>>>
    {
        public Guid? CategoryId { get; set; }
    }
}
