using Welco.Shared.Enums;

namespace Welco.Shared.Common.DTOs.UserManagement
{
    public class CompanyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public CompanyType Type { get; set; }
        public Guid CountryId { get; set; }
        public string? CountryNameEn { get; set; }
        public int TierLevel { get; set; }
        public CompanyStatus Status { get; set; }
        public Guid? AccountManagerId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
