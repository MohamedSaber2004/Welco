using MediatR;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Results;

namespace Content.Services.API.Features.LandingPages.Queries.GetLandingPageBySlug
{
    public class GetLandingPageBySlugQuery : IRequest<Result<LandingPageDto>>
    {
        public string Slug { get; set; } = string.Empty;
    }
}
