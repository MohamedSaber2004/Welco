namespace Welco.Shared.Common.DTOs.UserManagement
{
    public class CompanyAddressDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }

        public Guid CountryId { get; set; }
        public string? CountryNameEn { get; set; }
        public string? CountryNameAr { get; set; }
        public string? CountryCode { get; set; }
        public string? CountryPhoneCode { get; set; }

        public Guid CityId { get; set; }
        public string? CityNameEn { get; set; }
        public string? CityNameAr { get; set; }

        public Guid ZoneId { get; set; }
        public string? ZoneNameEn { get; set; }
        public string? ZoneNameAr { get; set; }

        public string Street { get; set; } = null!;
        public string? Building { get; set; }
        public string? Floor { get; set; }
        public string? Apartment { get; set; }
        public bool IsDefault { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
