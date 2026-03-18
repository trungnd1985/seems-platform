using Seems.Domain.Common;
using Seems.Domain.Enums;

namespace Seems.Domain.Entities;

public class SlotMapping : BaseEntity
{
    public Guid PageId { get; set; }
    public string SlotKey { get; set; } = string.Empty;
    public SlotTargetType TargetType { get; set; }
    public string TargetId { get; set; } = string.Empty;
    public int Order { get; set; }
    /// <summary>Arbitrary JSON object of per-slot configuration values (stored as jsonb).</summary>
    public string? Parameters { get; set; }

    public Page Page { get; set; } = null!;
}
