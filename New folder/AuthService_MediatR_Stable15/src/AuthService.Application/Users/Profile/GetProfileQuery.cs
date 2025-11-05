using AuthService.Contracts.Users;
using MediatR;
namespace AuthService.Application.Users.Profile;

public sealed record GetProfileQuery(Guid UserId) : IRequest<ProfileResponse>;