using Welco.Shared.Common.Classes;

namespace Welco.Shared.Domain.Models
{
    public class Provider : BaseEntity<Guid>
    {
        public string CommercialName { get; set; } = null!;
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

        public static Provider Create(
            string commercialName,
            string? companyName,
            string? commercialRegistrationNumber,
            string? contactPersonName,
            string? contactPersonPhone,
            string? phone,
            string? email,
            string? address,
            string? notes,
            string? imageName,
            Guid? ownerUserId,
            string createdBy)
        {
            var provider = new Provider
            {
                Id = Guid.NewGuid(),
                CommercialName = commercialName,
                CompanyName = companyName,
                CommercialRegistrationNumber = commercialRegistrationNumber,
                ContactPersonName = contactPersonName,
                ContactPersonPhone = contactPersonPhone,
                Phone = phone,
                Email = email,
                Address = address,
                Notes = notes,
                ImageName = imageName,
                OwnerUserId = ownerUserId
            };
            provider.MarkAsCreated(createdBy);
            return provider;
        }

        public void Update(
            string? commercialName,
            string? companyName,
            string? commercialRegistrationNumber,
            string? contactPersonName,
            string? contactPersonPhone,
            string? phone,
            string? email,
            string? address,
            string? notes,
            string? imageName,
            Guid? ownerUserId,
            string updatedBy)
        {
            if (!string.IsNullOrWhiteSpace(commercialName)) CommercialName = commercialName.Trim();
            if (companyName != null) CompanyName = string.IsNullOrWhiteSpace(companyName) ? null : companyName.Trim();
            if (commercialRegistrationNumber != null) CommercialRegistrationNumber = string.IsNullOrWhiteSpace(commercialRegistrationNumber) ? null : commercialRegistrationNumber.Trim();
            if (contactPersonName != null) ContactPersonName = string.IsNullOrWhiteSpace(contactPersonName) ? null : contactPersonName.Trim();
            if (contactPersonPhone != null) ContactPersonPhone = string.IsNullOrWhiteSpace(contactPersonPhone) ? null : contactPersonPhone.Trim();
            if (phone != null) Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
            if (email != null) Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
            if (address != null) Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
            if (notes != null) Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
            if (imageName != null) ImageName = string.IsNullOrWhiteSpace(imageName) ? null : imageName.Trim();
            if (ownerUserId.HasValue) OwnerUserId = ownerUserId;
            MarkAsUpdated(updatedBy);
        }
    }
}
