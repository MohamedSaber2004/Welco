using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Cities.Commands.CreateCity
{
    public class CreateCityCommand : IRequest<Result<CityDto>>
    {
        public Guid CountryId { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
    }
}
