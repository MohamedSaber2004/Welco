using MediatR;
using Welco.Shared.Common.DTOs.Auth.Responses;
using Welco.Shared.Results;

namespace Auth.Services.API.Features.Auth.Queries.GetUserProfile
{
    public class GetUserProfileQuery : IRequest<Result<UserProfileDto>>
    {
    }

}
