using AuthService.Domain.Common;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Interfaces;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Authentication.Commands;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        ILogger<RegisterCommandHandler> logger)
    {
        _userManager = userManager;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result<RegisterResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser is not null)
            {
                return Result<RegisterResponse>.Failure(new Error("Auth.EmailExists", "Email is already registered"));
            }

            var existingUsername = await _userManager.FindByNameAsync(request.UserName);
            if (existingUsername is not null)
            {
                return Result<RegisterResponse>.Failure(new Error("Auth.UsernameExists", "Username is already taken"));
            }

            // Create new user
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                UserName = request.UserName,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                EmailConfirmed = false,
                Status = UserStatus.Inactive,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("User registration failed for {Email}: {Errors}", request.Email, errors);
                return Result<RegisterResponse>.Failure(new Error("Auth.RegistrationFailed", errors));
            }

            // Add user to Customer role by default
            await _userManager.AddToRoleAsync(user, "Customer");

            // Generate email confirmation token
            var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            
            // TODO: Send confirmation email
            // await _emailService.SendEmailConfirmationAsync(user.Email!, $"https://yourapp.com/confirm-email?userId={user.Id}&token={emailToken}");

            _logger.LogInformation("User {Email} registered successfully with ID {UserId}", request.Email, user.Id);

            var response = new RegisterResponse(
                user.Id,
                user.Email!,
                user.UserName!,
                "Registration successful. Please check your email to confirm your account.",
                emailToken
            );

            return Result<RegisterResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during user registration for {Email}", request.Email);
            return Result<RegisterResponse>.Failure(new Error("Auth.RegistrationError", "An error occurred during registration"));
        }
    }
}
