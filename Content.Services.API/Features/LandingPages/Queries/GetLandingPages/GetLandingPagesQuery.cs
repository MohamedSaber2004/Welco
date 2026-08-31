using MediatR;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Results;

namespace Content.Services.API.Features.LandingPages.Queries.GetLandingPages
{
    public class GetLandingPagesQuery : IRequest<PaginatedResult<LandingPageDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Type { get; set; }
    }
}
