using Welco.Shared.Enums;

namespace Welco.Shared.Common.DTOs.UserManagement
{
    public class DistributorApplicationDto
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public CompanyType Type { get; set; } = CompanyType.Distributor;
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
        public ApplicantUserDto? ApplicantUser { get; set; }
    }

    public class ApplicantUserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public UserType UserType { get; set; }
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
