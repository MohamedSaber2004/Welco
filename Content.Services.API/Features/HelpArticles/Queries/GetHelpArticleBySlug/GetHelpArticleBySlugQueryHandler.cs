using Content.Services.API.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using HelpArticleEntity = Welco.Shared.Domain.Models.HelpArticle;

namespace Content.Services.API.Features.HelpArticles.Queries.GetHelpArticleBySlug
{
    public class GetHelpArticleBySlugQueryHandler : IRequestHandler<GetHelpArticleBySlugQuery, Result<HelpArticleDto>>
    {
        private readonly IUnitOfWork _uow;
        public GetHelpArticleBySlugQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Result<HelpArticleDto>> Handle(GetHelpArticleBySlugQuery request, CancellationToken cancellationToken)
        {
            var slug = request.Slug.Trim().ToLowerInvariant();
            var repo = _uow.GetRepository<HelpArticleEntity, Guid>();
            var dto = await repo.GetAll(a => !a.IsDeleted && a.Slug.ToLower() == slug)
                .Select(ContentDtoMapper.HelpArticleProjection)
                .FirstOrDefaultAsync(cancellationToken);
            if (dto == null) return Result<HelpArticleDto>.NotFound(LocalizationKeys.HelpArticle.NotFound);
            return Result<HelpArticleDto>.Success(dto, LocalizationKeys.HelpArticle.Fetched);
        }
    }
}
