namespace Welco.Shared.Common.DTOs.UserManagement
{
    public class CityDto
    {
        public Guid Id { get; set; }
        public Guid CountryId { get; set; }
        public string? CountryNameEn { get; set; }
        public string? CountryNameAr { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
