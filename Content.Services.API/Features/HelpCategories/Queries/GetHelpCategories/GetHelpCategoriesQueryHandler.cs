using Content.Services.API.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using HelpCategoryEntity = Welco.Shared.Domain.Models.HelpCategory;

namespace Content.Services.API.Features.HelpCategories.Queries.GetHelpCategories
{
    public class GetHelpCategoriesQueryHandler : IRequestHandler<GetHelpCategoriesQuery, Result<List<HelpCategoryDto>>>
    {
        private readonly IUnitOfWork _uow;
        public GetHelpCategoriesQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Result<List<HelpCategoryDto>>> Handle(GetHelpCategoriesQuery request, CancellationToken cancellationToken)
        {
            var repo = _uow.GetRepository<HelpCategoryEntity, Guid>();
            var list = await repo.GetAll(c => !c.IsDeleted)
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(ContentDtoMapper.HelpCategoryProjection)
                .ToListAsync(cancellationToken);
            return Result<List<HelpCategoryDto>>.Success(list, LocalizationKeys.HelpCategory.ListFetched);
        }
    }
}
