using MediatR;
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

    public class DistributorApplicationDto
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public Guid CountryId { get; set; }
        public string? CountryNameEn { get; set; }
        public string SalesVolumeBand { get; set; } = string.Empty;
        public string? Website { get; set; }
        public string ContactPerson { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
    }
}
