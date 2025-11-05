namespace AuthService.Domain.Common;

/// <summary>
/// Base entity with common properties
/// </summary>
public abstract class BaseEntity
{
    public required Guid Id { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; init; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Base auditable entity with soft delete support
/// </summary>
public abstract class BaseAuditableEntity : BaseEntity
{
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
