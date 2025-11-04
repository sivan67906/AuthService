using AuthService.Domain.Common;
using AuthService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Services;

public sealed class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task<Result> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        try
        {
            // TODO: Implement actual email sending using SendGrid, AWS SES, or SMTP
            _logger.LogInformation("Sending email to {To} with subject {Subject}", to, subject);
            
            // Simulate email sending
            await Task.Delay(100, cancellationToken);
            
            _logger.LogInformation("Email sent successfully to {To}", to);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            return Result.Failure(new Error("Email.SendFailed", $"Failed to send email: {ex.Message}"));
        }
    }

    public async Task<Result> SendEmailConfirmationAsync(string email, string confirmationLink, CancellationToken cancellationToken = default)
    {
        var subject = "Confirm Your Email Address";
        var body = $@"
            <h2>Welcome to AuthService!</h2>
            <p>Please confirm your email address by clicking the link below:</p>
            <a href='{confirmationLink}'>Confirm Email</a>
            <p>If you didn't create an account, please ignore this email.</p>
        ";

        return await SendEmailAsync(email, subject, body, cancellationToken);
    }

    public async Task<Result> SendPasswordResetAsync(string email, string resetLink, CancellationToken cancellationToken = default)
    {
        var subject = "Reset Your Password";
        var body = $@"
            <h2>Password Reset Request</h2>
            <p>Click the link below to reset your password:</p>
            <a href='{resetLink}'>Reset Password</a>
            <p>This link will expire in 1 hour.</p>
            <p>If you didn't request a password reset, please ignore this email.</p>
        ";

        return await SendEmailAsync(email, subject, body, cancellationToken);
    }

    public async Task<Result> SendTwoFactorCodeAsync(string email, string code, CancellationToken cancellationToken = default)
    {
        var subject = "Your Two-Factor Authentication Code";
        var body = $@"
            <h2>Two-Factor Authentication</h2>
            <p>Your verification code is: <strong>{code}</strong></p>
            <p>This code will expire in 5 minutes.</p>
            <p>If you didn't request this code, please contact support immediately.</p>
        ";

        return await SendEmailAsync(email, subject, body, cancellationToken);
    }
}

public sealed class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;

    public SmsService(ILogger<SmsService> logger)
    {
        _logger = logger;
    }

    public async Task<Result> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            // TODO: Implement actual SMS sending using Twilio, AWS SNS, or another provider
            _logger.LogInformation("Sending SMS to {PhoneNumber}", phoneNumber);
            
            // Simulate SMS sending
            await Task.Delay(100, cancellationToken);
            
            _logger.LogInformation("SMS sent successfully to {PhoneNumber}", phoneNumber);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", phoneNumber);
            return Result.Failure(new Error("Sms.SendFailed", $"Failed to send SMS: {ex.Message}"));
        }
    }

    public async Task<Result> SendTwoFactorCodeAsync(string phoneNumber, string code, CancellationToken cancellationToken = default)
    {
        var message = $"Your verification code is: {code}. This code will expire in 5 minutes.";
        return await SendSmsAsync(phoneNumber, message, cancellationToken);
    }
}
