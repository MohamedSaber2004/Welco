namespace Welco.Shared.Common.DTOs.UserManagement
{
    public class CountryDto
    {
        public Guid Id { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? PhoneCode { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
