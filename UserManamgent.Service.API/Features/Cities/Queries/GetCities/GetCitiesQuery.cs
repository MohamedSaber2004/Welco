using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Cities.Queries.GetCities
{
    public class GetCitiesQuery : IRequest<Result<IReadOnlyList<CityDto>>>
    {
        public Guid? CountryId { get; set; }
    }
}
