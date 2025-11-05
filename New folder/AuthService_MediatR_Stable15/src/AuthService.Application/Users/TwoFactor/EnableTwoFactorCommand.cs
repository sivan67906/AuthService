using MediatR;
namespace AuthService.Application.Users.TwoFactor; public sealed record EnableTwoFactorCommand(Guid UserId) : IRequest<string?>;