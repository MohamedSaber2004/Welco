namespace Welco.Shared.Common.DTOs.Providers
{
    public class ProviderDto
    {
        public Guid Id { get; set; }
        public string CommercialName { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public string? CommercialRegistrationNumber { get; set; }
        public string? ContactPersonName { get; set; }
        public string? ContactPersonPhone { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Notes { get; set; }
        public string? ImageName { get; set; }
        public Guid? OwnerUserId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
