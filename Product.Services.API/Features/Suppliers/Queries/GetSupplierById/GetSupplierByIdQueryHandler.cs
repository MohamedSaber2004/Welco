using MediatR;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Results;
using Welco.Shared.Localization;

namespace Product.Services.API.Features.Suppliers.Queries.GetSupplierById
{
    public class GetSupplierByIdQueryHandler : IRequestHandler<GetSupplierByIdQuery, Result<SupplierDto>>
    {
        public async Task<Result<SupplierDto>> Handle(GetSupplierByIdQuery request, CancellationToken cancellationToken)
        {
            var handler = new GetSuppliers.GetSuppliersQueryHandler();
            var result = await handler.Handle(new GetSuppliers.GetSuppliersQuery(), cancellationToken);
            var supplier = result.Data?.FirstOrDefault(s => s.Id == request.Id);
            if (supplier == null) return Result<SupplierDto>.NotFound(LocalizationKeys.Product.NotFound);
            return Result<SupplierDto>.Success(supplier, "Supplier fetched");
        }
    }
}
