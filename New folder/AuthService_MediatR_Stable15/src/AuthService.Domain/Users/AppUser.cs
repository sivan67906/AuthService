using Microsoft.AspNetCore.Identity;
namespace AuthService.Domain.Users;

public sealed class AppUser : IdentityUser<Guid>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedUtc { get; init; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public ICollection<UserAddress> Addresses { get; set; } = new List<UserAddress>();
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresUtc { get; set; }
}
