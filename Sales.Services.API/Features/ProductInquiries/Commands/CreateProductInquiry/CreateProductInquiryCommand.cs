using MediatR;
using Welco.Shared.Common.DTOs.Sales;
using Welco.Shared.Results;

namespace Sales.Services.API.Features.ProductInquiries.Commands.CreateProductInquiry
{
    public class CreateProductInquiryCommand : IRequest<Result<ProductInquiryDto>>
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Organization { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Email { get; set; }
    }
}
