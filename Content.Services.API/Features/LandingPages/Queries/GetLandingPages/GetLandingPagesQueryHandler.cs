using Content.Services.API.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Common.Extensions;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using LandingPageEntity = Welco.Shared.Domain.Models.LandingPage;

namespace Content.Services.API.Features.LandingPages.Queries.GetLandingPages
{
    public class GetLandingPagesQueryHandler : IRequestHandler<GetLandingPagesQuery, PaginatedResult<LandingPageDto>>
    {
        private readonly IUnitOfWork _uow;
        public GetLandingPagesQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<PaginatedResult<LandingPageDto>> Handle(GetLandingPagesQuery request, CancellationToken cancellationToken)
        {
            var repo = _uow.GetRepository<LandingPageEntity, Guid>();
            var query = repo.GetAll(x => !x.IsDeleted).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Type))
                query = query.Where(x => x.Type.ToLower() == request.Type.Trim().ToLower());

            return await query.OrderByDescending(x => x.CreatedAt)
                .ToPaginatedListAsync(ContentDtoMapper.LandingPageProjection, request.PageNumber, request.PageSize, LocalizationKeys.LandingPage.ListFetched, cancellationToken);
        }
    }
}
