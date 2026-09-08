using MediatR;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Results;

namespace Content.Services.API.Features.LandingPages.Commands.DeleteLandingPage
{
    public class DeleteLandingPageCommand : IRequest<Result<string>>
    {
        public Guid Id { get; set; }
    }
}
