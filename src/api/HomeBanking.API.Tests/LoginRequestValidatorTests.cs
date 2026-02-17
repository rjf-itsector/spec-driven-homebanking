using FluentAssertions;
using HomeBanking.API.Validators;
using HomeBanking.Contracts.Requests;

namespace HomeBanking.API.Tests;

/// <summary>
/// Tests for LoginRequestValidator (FluentValidation rules).
/// </summary>
public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void ValidRequest_PassesValidation()
    {
        var request = new LoginRequest("user@example.com", "SecurePass123");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyEmail_FailsValidation(string email)
    {
        var request = new LoginRequest(email, "password");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@")]
    [InlineData("@nodomain")]
    public void InvalidEmail_FailsValidation(string email)
    {
        var request = new LoginRequest(email, "password");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyPassword_FailsValidation(string password)
    {
        var request = new LoginRequest("user@example.com", password);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }
}
