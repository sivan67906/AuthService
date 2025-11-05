using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
namespace AuthService.Infrastructure.Email;

public sealed class EmailSender(IHttpClientFactory httpClientFactory, ILogger<EmailSender> logger) : AuthService.Abstractions.Email.IEmailSender
{
    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        var client = httpClientFactory.CreateClient("email");
        try { var payload = new { to, subject, htmlBody }; await client.PostAsJsonAsync("/send", payload, ct); }
        catch (Exception ex) { logger.LogError(ex, "Failed to send email"); }
    }
}
