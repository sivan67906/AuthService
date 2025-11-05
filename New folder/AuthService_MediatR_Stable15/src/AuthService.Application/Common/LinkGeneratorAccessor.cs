using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
namespace AuthService.Application;

public sealed class LinkGeneratorAccessor(LinkGenerator linkGenerator, IHttpContextAccessor http)
{
    public string BuildConfirmEmailUrl(Guid userId, string token)
    {
        var ctx = http.HttpContext!; var req = ctx.Request;
        var uri = linkGenerator.GetUriByAction(ctx, "ConfirmEmail", "Register", new { userId, token }, scheme: req.Scheme, host: new HostString(req.Host.Host, req.Host.Port ?? (req.IsHttps ? 443 : 80)));
        return uri!;
    }
}
