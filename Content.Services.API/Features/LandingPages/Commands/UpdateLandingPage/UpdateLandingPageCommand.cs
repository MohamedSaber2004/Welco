using MediatR;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Results;

namespace Content.Services.API.Features.LandingPages.Commands.UpdateLandingPage
{
    public class UpdateLandingPageCommand : IRequest<Result<LandingPageDto>>
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string HeroTitle { get; set; } = string.Empty;
        public string? HeroBody { get; set; }
        public string? ContentBlock { get; set; }
        public bool? IsActive { get; set; }
    }
}
