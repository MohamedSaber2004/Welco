namespace Welco.Shared.Common.DTOs.Certifications
{
    public class CertificationDto
    {
        public Guid Id { get; set; }
        public string CertificateNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string IssuedTo { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? Description { get; set; }
        public string? CertificationImageName { get; set; }
        public Guid? OwnerUserId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
