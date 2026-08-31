using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Results;
using Welco.Shared.Enums;
namespace UserManamgent.Service.API.Features.Companies.Commands.UpdateCompany
{
    public class UpdateCompanyCommand : IRequest<Result<CompanyDto>>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public CompanyType Type { get; set; }
        public Guid CountryId { get; set; }
        public int TierLevel { get; set; }
        public CompanyStatus Status { get; set; }
        public Guid? AccountManagerId { get; set; }
        public bool? IsActive { get; set; }
    }
}
