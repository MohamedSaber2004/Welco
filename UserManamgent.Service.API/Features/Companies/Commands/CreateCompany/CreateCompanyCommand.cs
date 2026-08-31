using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Results;
using Welco.Shared.Enums;
namespace UserManamgent.Service.API.Features.Companies.Commands.CreateCompany
{
    public class CreateCompanyCommand : IRequest<Result<CompanyDto>>
    {
        public string Name { get; set; } = string.Empty;
        public CompanyType Type { get; set; }
        public Guid CountryId { get; set; }
        public int TierLevel { get; set; } = 1;
        public CompanyStatus Status { get; set; } = CompanyStatus.Pending;
        public Guid? AccountManagerId { get; set; }
    }
}
