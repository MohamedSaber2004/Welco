using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Integration;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Results;
using CategoryEntity = Welco.Shared.Domain.Models.Category;

namespace Product.Services.API.Features.Integration.Categories.Queries.GetExternalCategories
{
    public class GetExternalCategoriesQuery : IRequest<Result<List<ExternalCategoryDto>>>
    {
    }

    public class GetExternalCategoriesQueryHandler : IRequestHandler<GetExternalCategoriesQuery, Result<List<ExternalCategoryDto>>>
    {
        private readonly IUnitOfWork _uow;

        public GetExternalCategoriesQueryHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<Result<List<ExternalCategoryDto>>> Handle(GetExternalCategoriesQuery request, CancellationToken ct)
        {
            var categoryRepo = _uow.GetRepository<CategoryEntity, Guid>();
            var categories = await categoryRepo
                .GetAll(c => !c.IsDeleted)
                .OrderBy(c => c.NameEn)
                .Select(c => new ExternalCategoryDto
                {
                    Id = c.Id,
                    NameEn = c.NameEn,
                    NameAr = c.NameAr,
                    Description = c.Description,
                    ImageName = c.ImageName,
                    ParentCategoryId = c.ParentCategoryId
                })
                .AsNoTracking()
                .ToListAsync(ct);

            return Result<List<ExternalCategoryDto>>.Success(categories);
        }
    }
}
