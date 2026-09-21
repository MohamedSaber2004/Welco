using MediatR;
using Product.Services.API.Features.Shared;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using ProductEntity = Welco.Shared.Domain.Models.Product;

namespace Product.Services.API.Features.Products.Commands.DeleteProduct
{
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result<string>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public DeleteProductCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<string>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var productRepo = _unitOfWork.GetRepository<ProductEntity, Guid>();
            var product = await productRepo.GetByIdAsync(request.Id, cancellationToken);

            if (product == null || product.IsDeleted)
            {
                return Result<string>.NotFound(LocalizationKeys.Product.NotFound);
            }

            // Mediator model: providers may only delete their own company's
            // products (see UpdateProductCommandHandler).
            var scope = await ProviderScope.GetAsync(_unitOfWork, _currentUserService, cancellationToken);
            if (scope.IsOrganizationUser &&
                (!scope.CompanyId.HasValue || product.CompanyId != scope.CompanyId.Value))
            {
                return Result<string>.NotFound(LocalizationKeys.Product.NotFound);
            }

            var currentUserId = _currentUserService.UserId != Guid.Empty
                ? _currentUserService.UserId.ToString()
                : "System";

            product.MarkAsDeleted(currentUserId);
            productRepo.Update(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(product.Id.ToString(), LocalizationKeys.Product.Deleted);
        }
    }
}
