using Welco.Shared.Common.Classes;

namespace Welco.Shared.Domain.Models
{
    public class AuditLog : BaseEntity<Guid>
    {
        public string EntityName { get; set; } = null!;
        public string EntityId { get; set; } = null!;
        public string Action { get; set; } = null!;
        public string? Details { get; set; }
        public string? PerformedBy { get; set; }
    }
}
