using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.CompanyAddresses.Queries.GetCompanyAddressById
{
    public class GetCompanyAddressByIdQuery : IRequest<Result<CompanyAddressDto>>
    {
        public Guid Id { get; set; }
    }
}
