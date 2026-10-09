using MediatR;
using Welco.Shared.Common.DTOs.Sales;
using Welco.Shared.Results;

namespace Sales.Services.API.Features.ProductInquiries.Commands.RespondProductInquiry
{
    public class RespondProductInquiryCommand : IRequest<Result<ProductInquiryDto>>
    {
        public Guid Id { get; set; }
        public string Response { get; set; } = string.Empty;
    }
}
