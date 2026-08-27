namespace Welco.Shared.Common.DTOs.UserManagement
{
    public class ZoneDto
    {
        public Guid Id { get; set; }
        public Guid CityId { get; set; }
        public string? CityNameEn { get; set; }
        public string? CityNameAr { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
