using MediatR;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Results;

namespace Content.Services.API.Features.LandingPages.Commands.CreateLandingPage
{
    public class CreateLandingPageCommand : IRequest<Result<LandingPageDto>>
    {
        public string Type { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string HeroTitle { get; set; } = string.Empty;
        public string? HeroBody { get; set; }
        public string? ContentBlock { get; set; }
    }
}
