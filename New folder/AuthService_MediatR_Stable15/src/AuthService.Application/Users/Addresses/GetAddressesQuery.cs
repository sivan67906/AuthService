using AuthService.Contracts.Users;
using MediatR;
namespace AuthService.Application.Users.Addresses; public sealed record GetAddressesQuery(Guid UserId) : IRequest<IReadOnlyList<AddressResponse>>;