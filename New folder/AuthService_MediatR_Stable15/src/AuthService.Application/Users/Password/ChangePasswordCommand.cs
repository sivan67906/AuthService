using MediatR;
namespace AuthService.Application.Users.Password; public sealed record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword) : IRequest<bool>;