using MediatR;
namespace AuthService.Application.Users.Login; public sealed record RevokeCommand(Guid UserId) : IRequest<bool>;