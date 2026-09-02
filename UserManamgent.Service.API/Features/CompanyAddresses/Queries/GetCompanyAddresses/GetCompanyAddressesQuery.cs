using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.CompanyAddresses.Queries.GetCompanyAddresses
{
    public class GetCompanyAddressesQuery : IRequest<Result<IReadOnlyList<CompanyAddressDto>>>
    {
        public Guid CompanyId { get; set; }
    }
}
