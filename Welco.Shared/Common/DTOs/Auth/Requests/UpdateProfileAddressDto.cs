namespace Welco.Shared.Common.DTOs.Auth.Requests
{
    public class UpdateProfileAddressDto
    {
        public Guid? Id { get; set; }
        public Guid CountryId { get; set; }
        public Guid CityId { get; set; }
        public Guid ZoneId { get; set; }
        public string Street { get; set; } = string.Empty;
        public string? Building { get; set; }
        public string? Floor { get; set; }
        public string? Apartment { get; set; }
        public bool? IsDefault { get; set; }
    }
}
