using MediatR;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.CompanyAddresses.Commands.DeleteCompanyAddress
{
    public class DeleteCompanyAddressCommand : IRequest<Result<string>>
    {
        public Guid Id { get; set; }
    }
}
