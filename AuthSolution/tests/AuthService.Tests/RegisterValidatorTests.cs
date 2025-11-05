using AuthService.Application.Features.Auth;
using FluentAssertions;

namespace AuthService.Tests;

public class RegisterValidatorTests
{
    [Fact]
    public void Invalid_When_Email_Empty()
    {
        var v = new RegisterValidator();
        var result = v.Validate(new RegisterCommand("", "Password#123", "A", "B"));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Valid_When_Fields_Correct()
    {
        var v = new RegisterValidator();
        var result = v.Validate(new RegisterCommand("a@b.com", "Password#123", "A", "B"));
        result.IsValid.Should().BeTrue();
    }
}
