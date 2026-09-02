using Welco.Shared.Common.Classes;
using Welco.Shared.Enums;

namespace Welco.Shared.Domain.Models
{
    public class Company : BaseEntity<Guid>
    {
        public string Name { get; set; } = null!;
        public CompanyType Type { get; set; }
        public Guid CountryId { get; set; }
        public virtual Country? Country { get; set; }
        public int TierLevel { get; set; } = 1;
        public CompanyStatus Status { get; set; } = CompanyStatus.Pending;
        public Guid? AccountManagerId { get; set; }
        public virtual ApplicationUser? AccountManager { get; set; }
        public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public virtual ICollection<CompanyAddress> Addresses { get; set; } = new List<CompanyAddress>();

        public static Company Create(
            string name,
            CompanyType type,
            Guid countryId,
            int tierLevel,
            CompanyStatus status,
            Guid? accountManagerId,
            string createdBy)
        {
            var company = new Company
            {
                Id = Guid.NewGuid(),
                Name = name,
                Type = type,
                CountryId = countryId,
                TierLevel = tierLevel,
                Status = status,
                AccountManagerId = accountManagerId
            };
            company.MarkAsCreated(createdBy);
            return company;
        }

        public void Update(
            string name,
            CompanyType type,
            Guid countryId,
            int tierLevel,
            CompanyStatus status,
            Guid? accountManagerId,
            string updatedBy)
        {
            Name = name.Trim();
            Type = type;
            CountryId = countryId;
            TierLevel = tierLevel;
            Status = status;
            AccountManagerId = accountManagerId;
            MarkAsUpdated(updatedBy);
        }
    }
}
