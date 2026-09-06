using MediatR;
using Welco.Shared.Common.DTOs.Sales;
using Welco.Shared.Results;
namespace Sales.Services.API.Features.ProductInquiries.Queries.GetProductInquiryById
{
    public class GetProductInquiryByIdQuery : IRequest<Result<ProductInquiryDto>> { public Guid Id { get; set; } }
}
