using MediatR;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Results;

namespace Content.Services.API.Features.HelpArticles.Queries.GetHelpArticleById
{
    public class GetHelpArticleByIdQuery : IRequest<Result<HelpArticleDto>>
    {
        public Guid Id { get; set; }
    }
}
