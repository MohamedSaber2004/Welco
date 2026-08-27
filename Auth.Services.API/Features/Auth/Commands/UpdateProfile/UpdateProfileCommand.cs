using MediatR;
using Welco.Shared.Common.DTOs.Auth.Requests;
using Welco.Shared.Common.DTOs.Auth.Responses;
using Welco.Shared.Enums;
using Welco.Shared.Results;

namespace Auth.Services.API.Features.Auth.Commands.UpdateProfile
{
    public class UpdateProfileCommand : IRequest<Result<UserProfileDto>>
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ProfilePictureName { get; set; }
        public AppLanguage? Language { get; set; }
        public IList<UpdateProfileAddressDto>? Addresses { get; set; }
    }
}
