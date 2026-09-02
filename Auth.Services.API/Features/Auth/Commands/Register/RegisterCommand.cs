using MediatR;
using Welco.Shared.Enums;
using Welco.Shared.Results;

namespace Auth.Services.API.Features.Auth.Commands.Register
{
    public class RegisterCommand : IRequest<Result<string>>
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        /// <summary>
        /// Optional linkage to territory Country for phone code validation.
        /// When provided, phone must start with Country.PhoneCode (e.g. +971 for AE).
        /// Frontend sends this from locationService.findCountryByPhone.
        /// </summary>
        public Guid? PhoneCountryId { get; set; }
        public string? PhoneCountryCode { get; set; }
        public UserType UserType { get; set; } = UserType.OrganizationUser;
        public AppLanguage Language { get; set; } = AppLanguage.En;
    }
}
