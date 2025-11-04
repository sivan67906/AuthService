namespace AuthService.Domain.Enums;

public enum UserStatus
{
    Inactive = 0,
    Active = 1,
    Suspended = 2,
    Deleted = 3
}

public enum TwoFactorMethod
{
    None = 0,
    Email = 1,
    SMS = 2,
    Authenticator = 3
}

public enum ExternalProvider
{
    Google = 1,
    Microsoft = 2,
    Facebook = 3
}

public enum TokenType
{
    EmailConfirmation = 1,
    PasswordReset = 2,
    TwoFactor = 3
}
