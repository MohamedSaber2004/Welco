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
        public Guid? PhoneCountryId { get; set; }
        public string? PhoneCountryCode { get; set; }
        public UserType UserType { get; set; } = UserType.OrganizationUser;
        public AppLanguage Language { get; set; } = AppLanguage.En;

        public CompanyType? CompanyType { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyEmail { get; set; }
        public Guid? DistributorCountryId { get; set; }
        public string? SalesVolumeBand { get; set; }
        public string? CategoryInterest { get; set; }
        public string? Website { get; set; }
    }
}
