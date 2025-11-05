using System.Security.Claims;
using AuthService.Application.Users.Login;
using AuthService.Contracts.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace AuthService.Api.Features.Auth;

[ApiController]
[Route("api/auth")]
public sealed class LoginController(IMediator mediator) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        return Ok(await mediator.Send(new LoginCommand(request), ct));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] string refreshToken, CancellationToken ct)
    {
        return Ok(await mediator.Send(new RefreshCommand(refreshToken), ct));
    }

    [Authorize]
    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var ok = await mediator.Send(new RevokeCommand(userId), ct);
        return ok ? Ok(new { Message = "Tokens revoked" }) : NotFound();
    }
}
