using Welco.Shared.Common.Classes;

namespace Welco.Shared.Domain.Models
{
    public class Certification : BaseEntity<Guid>
    {
        public string CertificateNumber { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string IssuedTo { get; set; } = null!;
        public string Issuer { get; set; } = null!;
        public DateTime IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? Description { get; set; }
        public string? CertificationImageName { get; set; }
        public Guid? OwnerUserId { get; set; }

        public static Certification Create(
            string certificateNumber,
            string title,
            string issuedTo,
            string issuer,
            DateTime issueDate,
            DateTime? expiryDate,
            string? description,
            string? certificationImageName,
            Guid? ownerUserId,
            string createdBy)
        {
            var certification = new Certification
            {
                Id = Guid.NewGuid(),
                CertificateNumber = certificateNumber,
                Title = title,
                IssuedTo = issuedTo,
                Issuer = issuer,
                IssueDate = issueDate,
                ExpiryDate = expiryDate,
                Description = description,
                CertificationImageName = certificationImageName,
                OwnerUserId = ownerUserId
            };
            certification.MarkAsCreated(createdBy);
            return certification;
        }

        public void Update(
            string certificateNumber,
            string title,
            string issuedTo,
            string issuer,
            DateTime issueDate,
            DateTime? expiryDate,
            string? description,
            string? certificationImageName,
            string updatedBy)
        {
            CertificateNumber = certificateNumber.Trim();
            Title = title.Trim();
            IssuedTo = issuedTo.Trim();
            Issuer = issuer.Trim();
            IssueDate = issueDate;
            ExpiryDate = expiryDate;

            if (description != null)
                Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();

            if (certificationImageName != null)
                CertificationImageName = string.IsNullOrWhiteSpace(certificationImageName) ? null : certificationImageName.Trim();

            MarkAsUpdated(updatedBy);
        }
    }
}
