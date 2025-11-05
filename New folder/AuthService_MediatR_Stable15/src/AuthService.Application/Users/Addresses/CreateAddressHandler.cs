using AuthService.Abstractions.Repositories;
using AuthService.Domain.Users;
using MediatR;

namespace AuthService.Application.Users.Addresses;

public sealed class CreateAddressHandler(IUserRepository repo) : IRequestHandler<CreateAddressCommand, Guid>
{
    public async Task<Guid> Handle(CreateAddressCommand request, CancellationToken ct)
    {
        var a = new UserAddress
        {
            UserId = request.UserId,
            Line1 = request.Request.Line1,
            Line2 = request.Request.Line2,
            City = request.Request.City,
            State = request.Request.State,
            PostalCode = request.Request.PostalCode,
            Country = request.Request.Country
        };
        await repo.AddAddressAsync(a, ct);
        return a.Id;
    }
}
