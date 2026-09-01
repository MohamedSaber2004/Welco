using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.DistributorApplications.Commands.CreateDistributorApplication
{
    public class CreateDistributorApplicationCommand : IRequest<Result<DistributorApplicationDto>>
    {
        public string CompanyName { get; set; } = string.Empty;
        public Guid CountryId { get; set; }
        public string SalesVolumeBand { get; set; } = string.Empty;
        public string? CategoryInterest { get; set; }
        public string? Website { get; set; }
        public string ContactPerson { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
    }
}
