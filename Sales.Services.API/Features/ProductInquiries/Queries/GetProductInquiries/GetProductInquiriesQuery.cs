using MediatR;
using Welco.Shared.Results;
using Welco.Shared.Common.DTOs.Sales;
namespace Sales.Services.API.Features.ProductInquiries.Queries.GetProductInquiries
{
    public class GetProductInquiriesQuery : IRequest<PaginatedResult<ProductInquiryDto>> { public int PageNumber { get; set; } = 1; public int PageSize { get; set; } = 10; public string? SearchTerm { get; set; } public Guid? ProductId { get; set; } }
}
