using AuthService.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Domain.Entities;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Inactive;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? LastLoginIp { get; set; }
    public bool IsEmailVerified { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }
    public TwoFactorMethod TwoFactorMethod { get; set; } = TwoFactorMethod.None;
    public string? TwoFactorSecret { get; set; }
    public string? RefreshTokenHash { get; set; }
    
    // Navigation properties
    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<ExternalLogin> ExternalLogins { get; set; } = new List<ExternalLogin>();
    public ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();
    
    // Computed properties
    public string FullName => $"{FirstName} {LastName}".Trim();
    public bool IsActive => Status == UserStatus.Active;
}
