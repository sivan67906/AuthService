using AuthService.Contracts.Users;
using MediatR;
namespace AuthService.Application.Users.Login; public sealed record LoginCommand(LoginRequest Request) : IRequest<AuthResponse>;