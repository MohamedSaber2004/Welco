using Content.Services.API.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using HelpArticleEntity = Welco.Shared.Domain.Models.HelpArticle;

namespace Content.Services.API.Features.HelpArticles.Queries.GetHelpArticles
{
    public class GetHelpArticlesQueryHandler : IRequestHandler<GetHelpArticlesQuery, Result<List<HelpArticleDto>>>
    {
        private readonly IUnitOfWork _uow;
        public GetHelpArticlesQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Result<List<HelpArticleDto>>> Handle(GetHelpArticlesQuery request, CancellationToken cancellationToken)
        {
            var repo = _uow.GetRepository<HelpArticleEntity, Guid>();
            var q = repo.GetAll(a => !a.IsDeleted).AsNoTracking();
            if (request.CategoryId.HasValue)
                q = q.Where(a => a.CategoryId == request.CategoryId.Value);
            var list = await q.OrderByDescending(a => a.CreatedAt)
                .Select(ContentDtoMapper.HelpArticleProjection)
                .ToListAsync(cancellationToken);
            return Result<List<HelpArticleDto>>.Success(list, LocalizationKeys.HelpArticle.ListFetched);
        }
    }
}
