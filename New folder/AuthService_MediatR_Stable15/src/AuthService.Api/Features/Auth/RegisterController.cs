using AuthService.Application.Users.Register;
using AuthService.Contracts.Users;
using MediatR;
using Microsoft.AspNetCore.Mvc;
namespace AuthService.Api.Features.Auth;

[ApiController]
[Route("api/auth/register")]
public sealed class RegisterController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new RegisterCommand(request), ct);
        return Ok(new { Message = "User created. Please confirm email.", result.UserId, result.Email, result.Token });
    }
    [HttpGet("confirm")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] Guid userId, [FromQuery] string token, CancellationToken ct)
    {
        var ok = await mediator.Send(new ConfirmEmailCommand(userId, token), ct);
        return ok ? Ok("Email confirmed") : NotFound();
    }
}
