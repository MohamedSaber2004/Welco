using MediatR;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using CategoryEntity = Welco.Shared.Domain.Models.Category;

namespace Product.Services.API.Features.Categories.Commands.UpdateCategory
{
    public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Result<CategoryDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public UpdateCategoryCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<CategoryDto>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            var categoryRepo = _unitOfWork.GetRepository<CategoryEntity, Guid>();
            var category = await categoryRepo.GetByIdAsync(request.Id, cancellationToken);

            if (category == null || category.IsDeleted)
            {
                return Result<CategoryDto>.NotFound(LocalizationKeys.Category.NotFound);
            }

            if (request.ParentCategoryId.HasValue)
            {
                if (request.ParentCategoryId.Value == request.Id)
                {
                    return Result<CategoryDto>.BadRequest(LocalizationKeys.Category.ParentCategoryNotFound);
                }

                var parentExists = await categoryRepo.ExistsAsync(
                    c => !c.IsDeleted && c.Id == request.ParentCategoryId.Value,
                    cancellationToken);

                if (!parentExists)
                {
                    return Result<CategoryDto>.BadRequest(LocalizationKeys.Category.ParentCategoryNotFound);
                }
            }

            var currentUserId = _currentUserService.UserId != Guid.Empty
                ? _currentUserService.UserId.ToString()
                : "System";

            category.Update(
                request.NameEn.Trim(),
                request.NameAr.Trim(),
                request.Description,
                request.ImageName,
                request.ParentCategoryId,
                currentUserId);

            if (request.IsActive.HasValue)
            {
                category.SetActiveState(request.IsActive.Value, currentUserId);
            }

            categoryRepo.Update(category);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<CategoryDto>.Success(ToDto(category), LocalizationKeys.Category.Updated);
        }

        private static CategoryDto ToDto(CategoryEntity category)
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
