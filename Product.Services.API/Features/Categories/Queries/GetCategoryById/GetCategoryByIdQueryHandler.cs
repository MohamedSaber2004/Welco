using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using CategoryEntity = Welco.Shared.Domain.Models.Category;

namespace Product.Services.API.Features.Categories.Queries.GetCategoryById
{
    public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, Result<CategoryDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCategoryByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            var categoryRepo = _unitOfWork.GetRepository<CategoryEntity, Guid>();
            var dto = await categoryRepo.GetAll(c => !c.IsDeleted && c.Id == request.Id)
                .AsNoTracking()
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    NameEn = c.NameEn,
                    NameAr = c.NameAr,
                    Description = c.Description,
                    ImageName = c.ImageName,
                    ParentCategoryId = c.ParentCategoryId,
                    IsActive = c.IsActive,
                    ProductCount = c.Products.Count(p => !p.IsDeleted) +
                                   c.SubCategories.Where(sc => !sc.IsDeleted).SelectMany(sc => sc.Products).Count(p => !p.IsDeleted),
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (dto == null)
            {
                return Result<CategoryDto>.NotFound(LocalizationKeys.Category.NotFound);
            }

            return Result<CategoryDto>.Success(dto, LocalizationKeys.Category.Fetched);
        }

        internal static CategoryDto ToDto(CategoryEntity category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                NameEn = category.NameEn,
                NameAr = category.NameAr,
                Description = category.Description,
                ImageName = category.ImageName,
                ParentCategoryId = category.ParentCategoryId,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }
    }
}
