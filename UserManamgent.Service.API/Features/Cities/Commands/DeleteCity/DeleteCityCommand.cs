using MediatR;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Cities.Commands.DeleteCity
{
    public class DeleteCityCommand : IRequest<Result<string>>
    {
        public Guid Id { get; set; }
    }
}
