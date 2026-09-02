using Welco.Shared.Enums;

namespace Welco.Shared.Common.DTOs.UserManagement
{
    public class DistributorApplicationDto
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public Guid CountryId { get; set; }
        public string? CountryNameEn { get; set; }
        public string SalesVolumeBand { get; set; } = string.Empty;
        public string? CategoryInterest { get; set; }
        public string? Website { get; set; }
        public string ContactPerson { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
