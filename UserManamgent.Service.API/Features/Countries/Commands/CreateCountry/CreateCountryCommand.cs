using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Countries.Commands.CreateCountry
{
    public class CreateCountryCommand : IRequest<Result<CountryDto>>
    {
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? PhoneCode { get; set; }
    }
}
