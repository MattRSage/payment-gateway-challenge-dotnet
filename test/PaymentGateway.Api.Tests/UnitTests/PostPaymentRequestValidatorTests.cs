using FluentValidation.TestHelper;

using Moq;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Validators;

namespace PaymentGateway.Api.Tests.UnitTests;

public class PostPaymentRequestValidatorTests
{
    private readonly Mock<IClock> _mockClock = new();
    private readonly PostPaymentRequestValidator _validator;

    public PostPaymentRequestValidatorTests()
    {
        _mockClock.Setup(x => x.UtcNow).Returns(new DateTime(2023, 6, 1));
        _validator = new PostPaymentRequestValidator(_mockClock.Object);
    }

    [Fact]
    public void Should_Have_Error_When_CardNumber_Is_Empty()
    {
        var request = new PostPaymentRequest("", 4, 2025, "123", "GBP", 100);
        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.CardNumber)
            .WithErrorMessage("Card number is required.");
    }

    [Fact]
    public void Should_Have_Error_When_CardNumber_Is_Not_Numeric()
    {
        var request = new PostPaymentRequest("ABCDEFG12345678", 4, 2025, "123", "GBP", 100);
        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.CardNumber)
            .WithErrorMessage("Card number must contain only numeric characters.");
    }

    [Fact]
    public void Should_Have_Error_When_CardNumber_Is_Invalid_Length()
    {
        var request = new PostPaymentRequest("12345678", 4, 2025, "123", "GBP", 100);
        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.CardNumber)
            .WithErrorMessage("Card number must be between 14-19 characters long.");
    }

    [Fact]
    public void Should_Have_Error_When_ExpiryMonth_Is_Empty()
    {
        var request = new PostPaymentRequest("2222405343248877", 0, 2025, "123", "GBP", 100);
        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.ExpiryMonth)
            .WithErrorMessage("Expiry month is required.");
    }

    [Fact]
    public void Should_Have_Error_When_ExpiryMonth_Is_Invalid()
    {
        var request = new PostPaymentRequest("2222405343248877", 13, 2025, "123", "GBP", 100);
        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.ExpiryMonth)
            .WithErrorMessage("Expiry month must be between 1-12.");
    }

    [Fact]
    public void Should_Have_Error_When_ExpiryYear_Is_In_The_Past()
    {
        var request = new PostPaymentRequest("2222405343248877", 4, 2020, "123", "GBP", 100);
        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.ExpiryYear)
            .WithErrorMessage("Expiry year must be in the future.");
    }

    [Fact]
    public void Should_Have_Error_When_Currency_Is_Empty()
    {
        var request = new PostPaymentRequest("2222405343248877", 4, 2025, "123", "", 100);
        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Currency)
            .WithErrorMessage("Currency is required.");
    }

    [Fact]
    public void Should_Have_Error_When_Currency_Is_Invalid()
    {
        var request = new PostPaymentRequest("2222405343248877", 4, 2025, "123", "AUD", 100);
        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Currency)
            .WithErrorMessage("Invalid currency code.");
    }

    [Fact]
    public void Should_Have_Error_When_Amount_Is_Invalid()
    {
        var request = new PostPaymentRequest("2222405343248877", 4, 2025, "123", "GBP", 0);
        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Amount)
            .WithErrorMessage("Amount must be greater than 0.");
    }

    [Fact]
    public void Should_Have_Error_When_Cvv_Is_Empty()
    {
        var request = new PostPaymentRequest("2222405343248877", 4, 2025, "", "GBP", 100);
        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Cvv)
            .WithErrorMessage("CVV is required.");
    }

    [Fact]
    public void Should_Have_Error_When_Cvv_Is_Invalid_Length()
    {
        var request = new PostPaymentRequest("2222405343248877", 4, 2025, "12", "GBP", 100);
        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Cvv)
            .WithErrorMessage("CVV must be 3-4 characters long.");
    }

    [Fact]
    public void Should_Have_Error_When_Cvv_Is_Not_Numeric()
    {
        var request = new PostPaymentRequest("2222405343248877", 4, 2025, "ABC", "GBP", 100);
        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Cvv)
            .WithErrorMessage("CVV must contain only numeric characters.");
    }

    [Fact]
    public void Should_Not_Have_Any_Validation_Errors_When_Request_Is_Valid()
    {
        var request = new PostPaymentRequest("2222405343248877", 4, 2025, "123", "GBP", 100);
        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}