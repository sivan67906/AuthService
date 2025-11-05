using System.Security.Claims;
using AuthService.Application.Users.Addresses;
using AuthService.Contracts.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace AuthService.Api.Features.Users;

[ApiController]
[Authorize(Roles = "User,Admin")]
[Route("api/users/addresses")]
public sealed class AddressController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var items = await mediator.Send(new GetAddressesQuery(userId), ct);
        return Ok(items);
    }
    [HttpPost]
    public async Task<IActionResult> Create(AddressRequest request, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var id = await mediator.Send(new CreateAddressCommand(userId, request), ct);
        return CreatedAtAction(nameof(List), new { }, new { Id = id });
    }
}
