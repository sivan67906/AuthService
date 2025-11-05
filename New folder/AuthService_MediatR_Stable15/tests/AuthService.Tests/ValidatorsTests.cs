using AuthService.Application.Users;
using AuthService.Contracts.Users;
using FluentAssertions;
using Xunit;
namespace AuthService.Tests;

public class ValidatorsTests
{
    [Fact]
    public void RegisterValidator_Valid()
    {
        var v = new RegisterValidator();
        var r = v.Validate(new RegisterRequest("a@b.com", "Password123", "A", "B"));
        r.IsValid.Should().BeTrue();
    }
}
