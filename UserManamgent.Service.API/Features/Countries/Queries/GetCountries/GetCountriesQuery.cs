using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Countries.Queries.GetCountries
{
    public class GetCountriesQuery : IRequest<Result<IReadOnlyList<CountryDto>>>
    {
    }
}
