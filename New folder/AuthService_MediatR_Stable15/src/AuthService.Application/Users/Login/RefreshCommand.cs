using AuthService.Contracts.Users;
using MediatR;
namespace AuthService.Application.Users.Login; public sealed record RefreshCommand(string RefreshToken) : IRequest<AuthResponse>;