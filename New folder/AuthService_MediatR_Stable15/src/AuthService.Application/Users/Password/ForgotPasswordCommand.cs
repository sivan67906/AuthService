using MediatR;
namespace AuthService.Application.Users.Password; public sealed record ForgotPasswordCommand(string Email) : IRequest<string?>;