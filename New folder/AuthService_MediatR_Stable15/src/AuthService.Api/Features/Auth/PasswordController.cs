using System.Security.Claims;
using AuthService.Application.Users.Password;
using AuthService.Contracts.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace AuthService.Api.Features.Auth;

[ApiController]
[Route("api/auth/password")]
public sealed class PasswordController(IMediator mediator) : ControllerBase
{
    [HttpPost("forgot")]
    public async Task<IActionResult> Forgot(ForgotPasswordRequest request, CancellationToken ct)
    {
        var token = await mediator.Send(new ForgotPasswordCommand(request.Email), ct);
        return Ok(new { Message = "Reset token generated", token });
    }
    [HttpPost("reset")]
    public async Task<IActionResult> Reset(ResetPasswordRequest request, CancellationToken ct)
    {
        var ok = await mediator.Send(new ResetPasswordCommand(request.Email, request.Token, request.NewPassword), ct);
        return ok ? Ok() : BadRequest("Failed to reset password");
    }
    [Authorize]
    [HttpPost("change")]
    public async Task<IActionResult> Change(ChangePasswordRequest request, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var ok = await mediator.Send(new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword), ct);
        return ok ? Ok() : BadRequest("Failed to change password");
    }
}
