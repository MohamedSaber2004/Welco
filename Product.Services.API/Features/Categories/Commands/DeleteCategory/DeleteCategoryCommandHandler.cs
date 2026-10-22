using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using CategoryEntity = Welco.Shared.Domain.Models.Category;
using ProductEntity = Welco.Shared.Domain.Models.Product;

namespace Product.Services.API.Features.Categories.Commands.DeleteCategory
{
    public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result<string>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public DeleteCategoryCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<string>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            var categoryRepo = _unitOfWork.GetRepository<CategoryEntity, Guid>();
            var category = await categoryRepo.GetByIdAsync(request.Id, cancellationToken);

            if (category == null || category.IsDeleted)
            {
                return Result<string>.NotFound(LocalizationKeys.Category.NotFound);
            }

            var currentUserId = _currentUserService.UserId != Guid.Empty
                ? _currentUserService.UserId.ToString()
                : "System";

            category.MarkAsDeleted(currentUserId);
            categoryRepo.Update(category);

            // Cascading soft-delete: Delete all active products belonging to this category
            var productRepo = _unitOfWork.GetRepository<ProductEntity, Guid>();
            var productsInCategory = await productRepo.GetAll(p => !p.IsDeleted && p.CategoryId == category.Id)
                .ToListAsync(cancellationToken);

            foreach (var prod in productsInCategory)
            {
                prod.MarkAsDeleted(currentUserId);
                productRepo.Update(prod);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(category.Id.ToString(), LocalizationKeys.Category.Deleted);
        }
    }
}
