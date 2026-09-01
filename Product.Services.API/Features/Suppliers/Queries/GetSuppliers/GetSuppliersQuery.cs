using MediatR;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Results;

namespace Product.Services.API.Features.Suppliers.Queries.GetSuppliers
{
    public class GetSuppliersQuery : IRequest<Result<IEnumerable<SupplierDto>>>
    {
    }
}
