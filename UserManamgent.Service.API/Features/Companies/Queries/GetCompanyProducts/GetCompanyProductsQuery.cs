using MediatR;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Companies.Queries.GetCompanyProducts
{
    /// <summary>
    /// Public catalog of one provider company (mediator model).
    /// Only active, non-deleted items are ever exposed.
    /// </summary>
    public class GetCompanyProductsQuery : IRequest<PaginatedResult<ProductDto>>
    {
        public Guid CompanyId { get; set; }
        public Guid? CategoryId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
