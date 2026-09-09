using Welco.Shared.Enums;

namespace Welco.Shared.Common.DTOs.UserManagement
{
    public class CompanyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? ImageName { get; set; }
        public CompanyType Type { get; set; }
        public Guid CountryId { get; set; }
        public string? CountryNameEn { get; set; }
        public string? CountryNameAr { get; set; }
        public CompanyStatus Status { get; set; }
        public Guid? AccountManagerId { get; set; }
        public bool IsActive { get; set; }
        public bool IsProvider { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
