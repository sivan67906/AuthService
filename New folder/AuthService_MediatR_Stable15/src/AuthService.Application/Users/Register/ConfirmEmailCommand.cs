using MediatR;
namespace AuthService.Application.Users.Register; public sealed record ConfirmEmailCommand(Guid UserId, string Token) : IRequest<bool>;