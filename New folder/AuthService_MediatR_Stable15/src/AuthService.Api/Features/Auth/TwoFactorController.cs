using System.Security.Claims;
using AuthService.Application.Users.TwoFactor;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace AuthService.Api.Features.Auth;

[ApiController]
[Route("api/auth/2fa")]
[Authorize]
public sealed class TwoFactorController(IMediator mediator) : ControllerBase
{
    [HttpPost("enable")]
    public async Task<IActionResult> Enable(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var key = await mediator.Send(new EnableTwoFactorCommand(userId), ct);
        return key is null ? NotFound() : Ok(new { SharedKey = key });
    }
    [HttpPost("disable")]
    public async Task<IActionResult> Disable(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var ok = await mediator.Send(new DisableTwoFactorCommand(userId), ct);
        return ok ? Ok() : NotFound();
    }
}
