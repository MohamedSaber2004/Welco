using MediatR;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Results;
namespace Content.Services.API.Features.OemInquiries.Queries.GetOemInquiryById
{
    public class GetOemInquiryByIdQuery : IRequest<Result<OemInquiryDto>> { public Guid Id { get; set; } }
}
