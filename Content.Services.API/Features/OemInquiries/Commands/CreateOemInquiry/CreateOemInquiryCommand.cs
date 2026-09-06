using MediatR;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Results;
namespace Content.Services.API.Features.OemInquiries.Commands.CreateOemInquiry
{
    public class CreateOemInquiryCommand : IRequest<Result<OemInquiryDto>>
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
