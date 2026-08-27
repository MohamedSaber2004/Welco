using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Cities.Commands.UpdateCity
{
    public class UpdateCityCommand : IRequest<Result<CityDto>>
    {
        public Guid Id { get; set; }
        public Guid? CountryId { get; set; }
        public string? NameEn { get; set; }
        public string? NameAr { get; set; }
    }
}
