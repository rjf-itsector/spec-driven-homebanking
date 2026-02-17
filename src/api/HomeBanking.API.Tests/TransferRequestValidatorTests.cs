using FluentAssertions;
using HomeBanking.API.Validators;
using HomeBanking.Contracts.Requests;

namespace HomeBanking.API.Tests;

/// <summary>
/// Tests for TransferRequestValidator (FluentValidation rules).
/// </summary>
public class TransferRequestValidatorTests
{
    private readonly TransferRequestValidator _validator = new();

    private static TransferRequest CreateValidRequest() => new(
        FromAccountId: Guid.NewGuid(),
        ToAccountId: Guid.NewGuid(),
        Amount: 100m,
        Description: "Test transfer"
    );

    [Fact]
    public void ValidRequest_PassesValidation()
    {
        var request = CreateValidRequest();

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void EmptyFromAccountId_FailsValidation()
    {
        var request = CreateValidRequest() with { FromAccountId = Guid.Empty };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FromAccountId");
    }

    [Fact]
    public void EmptyToAccountId_FailsValidation()
    {
        var request = CreateValidRequest() with { ToAccountId = Guid.Empty };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ToAccountId");
    }

    [Fact]
    public void SameSourceAndDestination_FailsValidation()
    {
        var accountId = Guid.NewGuid();
        var request = CreateValidRequest() with
        {
            FromAccountId = accountId,
            ToAccountId = accountId
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "ToAccountId" &&
            e.ErrorMessage.Contains("same account"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void NonPositiveAmount_FailsValidation(decimal amount)
    {
        var request = CreateValidRequest() with { Amount = amount };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Amount");
    }

    [Fact]
    public void AmountExceedsMaxLimit_FailsValidation()
    {
        var request = CreateValidRequest() with { Amount = 1_000_001m };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Amount" &&
            e.ErrorMessage.Contains("maximum transfer limit"));
    }

    [Fact]
    public void AmountAtMaxLimit_PassesValidation()
    {
        var request = CreateValidRequest() with { Amount = 1_000_000m };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void DescriptionOver250Chars_FailsValidation()
    {
        var longDescription = new string('A', 251);
        var request = CreateValidRequest() with { Description = longDescription };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }

    [Fact]
    public void DescriptionAt250Chars_PassesValidation()
    {
        var description = new string('A', 250);
        var request = CreateValidRequest() with { Description = description };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }
}
