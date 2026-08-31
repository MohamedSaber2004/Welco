using Welco.Shared.Common.Classes;
namespace Welco.Shared.Domain.Models
{
    public class Incoterm : BaseEntity<Guid>
    {
        public string Code { get; set; } = null!; // EXW, FOB, CIF, DDP
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}
