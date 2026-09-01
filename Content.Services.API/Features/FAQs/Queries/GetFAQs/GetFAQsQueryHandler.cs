using Content.Services.API.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using FAQEntity = Welco.Shared.Domain.Models.FAQItem;

namespace Content.Services.API.Features.FAQs.Queries.GetFAQs
{
    public class GetFAQsQueryHandler : IRequestHandler<GetFAQsQuery, Result<List<FAQItemDto>>>
    {
        private readonly IUnitOfWork _uow;
        public GetFAQsQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Result<List<FAQItemDto>>> Handle(GetFAQsQuery request, CancellationToken cancellationToken)
        {
            var repo = _uow.GetRepository<FAQEntity, Guid>();
            var list = await repo.GetAll(f => !f.IsDeleted)
                .AsNoTracking()
                .OrderBy(f => f.SortOrder)
                .ThenBy(f => f.CreatedAt)
                .Select(ContentDtoMapper.FAQProjection)
                .ToListAsync(cancellationToken);
            return Result<List<FAQItemDto>>.Success(list, LocalizationKeys.FAQ.ListFetched);
        }
    }
}
