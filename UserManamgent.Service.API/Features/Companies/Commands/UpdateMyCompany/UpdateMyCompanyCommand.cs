using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Companies.Commands.UpdateMyCompany
{
    /// <summary>
    /// Self-service update of the organization belonging to the signed-in user.
    ///
    /// Deliberately exposes ONLY the fields a provider legitimately owns. The
    /// admin-controlled fields (Status, Type, AccountManagerId, IsActive,
    /// IsProvider) are intentionally absent from this contract so a provider
    /// cannot self-approve, reassign an account manager, re-classify the
    /// entity, or reactivate itself through this endpoint — the handler copies
    /// those straight through from the stored record.
    /// </summary>
    public class UpdateMyCompanyCommand : IRequest<Result<CompanyDto>>
    {
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public Guid CountryId { get; set; }

        /// <summary>
        /// Stored attachment name of the brand logo. Send an empty string to
        /// clear the logo; null leaves the current value untouched.
        /// </summary>
        public string? ImageName { get; set; }
    }
}
