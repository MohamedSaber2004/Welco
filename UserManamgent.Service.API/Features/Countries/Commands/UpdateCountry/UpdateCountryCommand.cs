using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Countries.Commands.UpdateCountry
{
    public class UpdateCountryCommand : IRequest<Result<CountryDto>>
    {
        public Guid Id { get; set; }
        public string? NameEn { get; set; }
        public string? NameAr { get; set; }
        public string? Code { get; set; }
    }
}
