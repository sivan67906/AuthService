using AuthService.Contracts.Users;
using MediatR;
namespace AuthService.Application.Users.Register;

public sealed record RegisterCommand(RegisterRequest Request) : IRequest<RegisterResult>;
public sealed record RegisterResult(Guid UserId, string Email, string Token);