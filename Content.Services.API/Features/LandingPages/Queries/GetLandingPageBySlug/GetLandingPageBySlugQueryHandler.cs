using Content.Services.API.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using LandingPageEntity = Welco.Shared.Domain.Models.LandingPage;

namespace Content.Services.API.Features.LandingPages.Queries.GetLandingPageBySlug
{
    public class GetLandingPageBySlugQueryHandler : IRequestHandler<GetLandingPageBySlugQuery, Result<LandingPageDto>>
    {
        private readonly IUnitOfWork _uow;
        public GetLandingPageBySlugQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Result<LandingPageDto>> Handle(GetLandingPageBySlugQuery request, CancellationToken cancellationToken)
        {
            var repo = _uow.GetRepository<LandingPageEntity, Guid>();
            var slug = request.Slug.Trim().ToLowerInvariant();
            var dto = await repo.GetAll(x => !x.IsDeleted && x.Slug.ToLower() == slug)
                .Select(ContentDtoMapper.LandingPageProjection)
                .FirstOrDefaultAsync(cancellationToken);

            if (dto == null)
                return Result<LandingPageDto>.NotFound(LocalizationKeys.LandingPage.NotFound);

            return Result<LandingPageDto>.Success(dto, LocalizationKeys.LandingPage.Fetched);
        }
    }
}
