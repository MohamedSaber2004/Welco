using MediatR;
using Welco.Shared.Results;

namespace Content.Services.API.Features.HelpArticles.Commands.DeleteHelpArticle
{
    public class DeleteHelpArticleCommand : IRequest<Result<string>>
    {
        public Guid Id { get; set; }
    }
}
