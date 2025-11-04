using AuthService.Application.Features.Authentication.Commands;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace AuthService.Tests.Features.Authentication;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator;

    public RegisterCommandValidatorTests()
    {
        _validator = new RegisterCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new RegisterCommand(
            Email: "test@example.com",
            UserName: "testuser",
            Password: "Test@1234",
            ConfirmPassword: "Test@1234",
            FirstName: "Test",
            LastName: "User",
            PhoneNumber: "+1234567890"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithInvalidEmail_ShouldHaveError(string? email)  // ← Add ? here
    {
        // Arrange
        var command = new RegisterCommand(
            Email: email!,  // ← Add ! operator to suppress warning in usage
            UserName: "testuser",
            Password: "Test@1234",
            ConfirmPassword: "Test@1234",
            FirstName: null,
            LastName: null,
            PhoneNumber: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("test@")]
    [InlineData("@example.com")]
    public void Validate_WithInvalidEmailFormat_ShouldHaveError(string email)
    {
        // Arrange
        var command = new RegisterCommand(
            Email: email,
            UserName: "testuser",
            Password: "Test@1234",
            ConfirmPassword: "Test@1234",
            FirstName: null,
            LastName: null,
            PhoneNumber: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("toolongusernamethatexceedsthemaximumlengthallowed123456")]
    public void Validate_WithInvalidUsernameLength_ShouldHaveError(string username)
    {
        // Arrange
        var command = new RegisterCommand(
            Email: "test@example.com",
            UserName: username,
            Password: "Test@1234",
            ConfirmPassword: "Test@1234",
            FirstName: null,
            LastName: null,
            PhoneNumber: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Theory]
    [InlineData("test user")] // Contains space
    [InlineData("test@user")] // Contains invalid character
    public void Validate_WithInvalidUsernameCharacters_ShouldHaveError(string username)
    {
        // Arrange
        var command = new RegisterCommand(
            Email: "test@example.com",
            UserName: username,
            Password: "Test@1234",
            ConfirmPassword: "Test@1234",
            FirstName: null,
            LastName: null,
            PhoneNumber: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Theory]
    [InlineData("short")] // Too short
    [InlineData("NoNumber!")] // Missing number
    [InlineData("nouppercase1!")] // Missing uppercase
    [InlineData("NOLOWERCASE1!")] // Missing lowercase
    [InlineData("NoSpecialChar1")] // Missing special character
    public void Validate_WithWeakPassword_ShouldHaveError(string password)
    {
        // Arrange
        var command = new RegisterCommand(
            Email: "test@example.com",
            UserName: "testuser",
            Password: password,
            ConfirmPassword: password,
            FirstName: null,
            LastName: null,
            PhoneNumber: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_WithMismatchedPasswords_ShouldHaveError()
    {
        // Arrange
        var command = new RegisterCommand(
            Email: "test@example.com",
            UserName: "testuser",
            Password: "Test@1234",
            ConfirmPassword: "Different@1234",
            FirstName: null,
            LastName: null,
            PhoneNumber: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
    }

    [Theory]
    [InlineData("123456789")] // Too short
    [InlineData("abc")]
    [InlineData("+")]
    public void Validate_WithInvalidPhoneNumber_ShouldHaveError(string phoneNumber)
    {
        // Arrange
        var command = new RegisterCommand(
            Email: "test@example.com",
            UserName: "testuser",
            Password: "Test@1234",
            ConfirmPassword: "Test@1234",
            FirstName: null,
            LastName: null,
            PhoneNumber: phoneNumber
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void Validate_WithNullPhoneNumber_ShouldNotHaveError()
    {
        // Arrange
        var command = new RegisterCommand(
            Email: "test@example.com",
            UserName: "testuser",
            Password: "Test@1234",
            ConfirmPassword: "Test@1234",
            FirstName: null,
            LastName: null,
            PhoneNumber: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }
}

public class RegisterCommandTests
{
    [Fact]
    public void RegisterCommand_ShouldBeRecord()
    {
        // Arrange & Act
        var command = new RegisterCommand(
            "test@example.com",
            "testuser",
            "Test@1234",
            "Test@1234",
            "Test",
            "User",
            null
        );

        // Assert
        command.Should().NotBeNull();
        command.Email.Should().Be("test@example.com");
        command.UserName.Should().Be("testuser");
    }

    [Fact]
    public void RegisterCommand_WithClause_ShouldCreateNewInstance()
    {
        // Arrange
        var original = new RegisterCommand(
            "test@example.com",
            "testuser",
            "Test@1234",
            "Test@1234",
            "Test",
            "User",
            null
        );

        // Act
        var modified = original with { FirstName = "Modified" };

        // Assert
        modified.FirstName.Should().Be("Modified");
        original.FirstName.Should().Be("Test");
        modified.Should().NotBeSameAs(original);
    }
}
