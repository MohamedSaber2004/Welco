namespace Welco.Shared.Common.DTOs.UserManagement
{
    public class AuditLogDto
    {
        public Guid Id { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Details { get; set; }
        public string PerformedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
