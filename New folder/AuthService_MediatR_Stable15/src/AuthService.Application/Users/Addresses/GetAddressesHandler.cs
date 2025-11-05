using AuthService.Abstractions.Repositories;
using AuthService.Contracts.Users;
using MediatR;

namespace AuthService.Application.Users.Addresses;

public sealed class GetAddressesHandler(IUserRepository repo) : IRequestHandler<GetAddressesQuery, IReadOnlyList<AddressResponse>>
{
    public async Task<IReadOnlyList<AddressResponse>> Handle(GetAddressesQuery request, CancellationToken ct)
    {
        var items = await repo.GetAddressesAsync(request.UserId, ct);
        return items.Select(a => new AddressResponse(a.Id, a.Line1, a.Line2, a.City, a.State, a.PostalCode, a.Country, a.CreatedUtc))
                    .ToList();
    }
}
