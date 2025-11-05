using MediatR;
namespace AuthService.Application.Users.Password; public sealed record ResetPasswordCommand(string Email, string Token, string NewPassword) : IRequest<bool>;