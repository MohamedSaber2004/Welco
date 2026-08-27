using MediatR;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Countries.Commands.DeleteCountry
{
    public class DeleteCountryCommand : IRequest<Result<string>>
    {
        public Guid Id { get; set; }
    }
}
