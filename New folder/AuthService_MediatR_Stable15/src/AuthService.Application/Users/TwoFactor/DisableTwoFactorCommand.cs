using MediatR;
namespace AuthService.Application.Users.TwoFactor; public sealed record DisableTwoFactorCommand(Guid UserId) : IRequest<bool>;