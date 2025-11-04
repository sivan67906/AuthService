using AuthService.Domain.Common;

namespace AuthService.Domain.Entities;

public sealed class Address : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public string Street { get; set; } = string.Empty;
    public string? Apartment { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsShipping { get; set; }
    public bool IsBilling { get; set; }
    
    // Navigation property
    public ApplicationUser User { get; set; } = null!;
}

public sealed class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string JwtId { get; set; } = string.Empty;
    public bool IsUsed { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }
    
    // Navigation property
    public ApplicationUser User { get; set; } = null!;
    
    // Computed properties
    public bool IsActive => !IsRevoked && !IsUsed && ExpiresAt > DateTime.UtcNow;
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}

public sealed class ExternalLogin : BaseEntity
{
    public Guid UserId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string ProviderKey { get; set; } = string.Empty;
    public string? ProviderDisplayName { get; set; }
    public string? Email { get; set; }
    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public ApplicationUser User { get; set; } = null!;
}

public sealed class UserSession : BaseEntity
{
    public Guid UserId { get; set; }
    public string SessionToken { get; set; } = string.Empty;
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? EndedAt { get; set; }
    
    // Navigation property
    public ApplicationUser User { get; set; } = null!;
}
