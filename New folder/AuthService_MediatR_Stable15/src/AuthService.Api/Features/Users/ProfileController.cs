using System.Security.Claims;
using AuthService.Application.Users.Profile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace AuthService.Api.Features.Users;

[ApiController]
[Authorize]
[Route("api/users")]
public sealed class ProfileController(IMediator mediator) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var dto = await mediator.Send(new GetProfileQuery(userId), ct);
        return Ok(dto);
    }
}
