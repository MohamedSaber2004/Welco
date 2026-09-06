using MediatR;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Results;
namespace Content.Services.API.Features.OemInquiries.Queries.GetOemInquiries
{
    public class GetOemInquiriesQuery : IRequest<PaginatedResult<OemInquiryDto>> { public int PageNumber { get; set; } = 1; public int PageSize { get; set; } = 10; public string? SearchTerm { get; set; } }
}
