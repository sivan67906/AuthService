using AuthService.Contracts;
using AuthService.Domain.Common;
using AuthService.Infrastructure.Persistence;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuthService.ReadModel.Queries;

public record GetProfile(Guid UserId) : IRequest<Result<AuthDtos.ProfileResponse>>;

public class GetProfileHandler : IRequestHandler<GetProfile, Result<AuthDtos.ProfileResponse>>
{
    private readonly ReadDbContext _db;
    public GetProfileHandler(ReadDbContext db) => _db = db;

    public async Task<Result<AuthDtos.ProfileResponse>> Handle(GetProfile request, CancellationToken ct)
    {
        var u = await _db.Users.AsNoTracking()
            .Include(x => x.Addresses)
            .FirstOrDefaultAsync(x => x.Id == request.UserId, ct);

        if (u is null) return Result<AuthDtos.ProfileResponse>.Failure(new("user.not_found","User not found"));

        var dto = new AuthDtos.ProfileResponse(
            u.Id, u.Email!, u.FirstName, u.LastName,
            u.Addresses.Adapt<IEnumerable<AuthDtos.AddressDto>>(),
            Array.Empty<string>()
        );

        return Result<AuthDtos.ProfileResponse>.Success(dto);
    }
}
