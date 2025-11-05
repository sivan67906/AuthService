using AuthService.Contracts.Users;
using MediatR;
namespace AuthService.Application.Users.Addresses; public sealed record CreateAddressCommand(Guid UserId, AddressRequest Request) : IRequest<Guid>;