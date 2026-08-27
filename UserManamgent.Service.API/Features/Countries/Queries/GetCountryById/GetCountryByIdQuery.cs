using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Countries.Queries.GetCountryById
{
    public class GetCountryByIdQuery : IRequest<Result<CountryDto>>
    {
        public Guid Id { get; set; }
    }
}
