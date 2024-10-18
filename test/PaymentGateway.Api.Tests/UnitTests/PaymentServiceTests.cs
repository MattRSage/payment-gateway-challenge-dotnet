using FluentAssertions;

using FluentValidation;
using FluentValidation.Results;

using Moq;

using PaymentGateway.Api.Domain.Payments;
using PaymentGateway.Api.Infrastructure.AcquiringBank;
using PaymentGateway.Api.Infrastructure.Payments;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests.UnitTests;

public class PaymentServiceTests
{
    private readonly Mock<IValidator<PostPaymentRequest>> _mockValidator = new();
    private readonly Mock<IAcquiringBankClient> _mockAcquiringBankClient = new();
    private readonly PaymentsRepository _paymentsRepository = new();
    private readonly PaymentService _paymentService;

    public PaymentServiceTests()
    {
        _paymentService = new PaymentService(
            _mockAcquiringBankClient.Object,
            _paymentsRepository,
            _mockValidator.Object);
    }

    [Fact]
    public async Task ProcessPayment_ValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        var request = CreateValidPostPaymentRequest();
        SetupValidatorSuccess();
        SetupAcquiringBankClientSuccess();

        // Act
        var result = await _paymentService.ProcessPayment(request);

        // Assert
        result.AsT0.Should().NotBeNull();
        result.AsT0.Payment.Should().NotBeNull();
        result.AsT0.Payment.Status.Should().Be(PaymentStatus.Authorized);
        _paymentsRepository.Payments.Should().HaveCount(1);
        _paymentsRepository.Payments[0].Status.Should().Be(PaymentStatus.Authorized);
    }

    [Fact]
    public async Task ProcessPayment_AcquiringBankDeclines_ReturnsSuccessResultWithDeclinedStatus()
    {
        // Arrange
        var request = CreateValidPostPaymentRequest();
        SetupValidatorSuccess();
        SetupAcquiringBankClientDeclined();

        // Act
        var result = await _paymentService.ProcessPayment(request);

        // Assert
        result.AsT0.Should().NotBeNull();
        result.AsT0.Payment.Should().NotBeNull();
        result.AsT0.Payment.Status.Should().Be(PaymentStatus.Declined);
        _paymentsRepository.Payments.Should().HaveCount(1);
        _paymentsRepository.Payments[0].Status.Should().Be(PaymentStatus.Declined);
    }

    [Fact]
    public async Task ProcessPayment_InvalidRequest_ReturnsRejectedResult()
    {
        // Arrange
        var request = CreateValidPostPaymentRequest();
        SetupValidatorFailure();

        // Act
        var result = await _paymentService.ProcessPayment(request);

        // Assert
        result.AsT1.Should().NotBeNull();
        result.AsT1.ValidationResult.Should().NotBeNull();
        result.AsT1.ValidationResult.IsValid.Should().BeFalse();
        _paymentsRepository.Payments.Should().HaveCount(1);
        _paymentsRepository.Payments[0].Status.Should().Be(PaymentStatus.Rejected);

    }

    [Fact]
    public async Task ProcessPayment_AcquiringBankError_ReturnsErrorResult()
    {
        // Arrange
        var request = CreateValidPostPaymentRequest();
        SetupValidatorSuccess();
        SetupAcquiringBankClientError();

        // Act
        var result = await _paymentService.ProcessPayment(request);

        // Assert
        result.AsT2.Should().NotBeNull();
        _paymentsRepository.Payments.Should().BeEmpty();
    }

    [Fact]
    public async Task ProcessPayment_ValidRequest_SetsCorrectPaymentDetails()
    {
        // Arrange
        var request = CreateValidPostPaymentRequest();
        SetupValidatorSuccess();
        SetupAcquiringBankClientSuccess();

        // Act
        var result = await _paymentService.ProcessPayment(request);

        // Assert
        result.AsT0.Should().NotBeNull();
        result.AsT0.Payment.Should().NotBeNull();
        var payment = result.AsT0.Payment;
        payment.LastFourCardDigits.Should().Be(request.CardNumber[^4..]);
        payment.ExpiryMonth.Should().Be(request.ExpiryMonth);
        payment.ExpiryYear.Should().Be(request.ExpiryYear);
        payment.Currency.Should().Be(request.Currency);
        payment.Amount.Should().Be(request.Amount);
        payment.AuthorizationCode.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetPayment_ExistingPayment_ReturnsPayment()
    {
        // Arrange
        var payment = new Payment(Guid.NewGuid(), PaymentStatus.Authorized, "1234", 12, 2025, "USD", 100);
        _paymentsRepository.Add(payment);

        // Act
        var result = _paymentService.GetPayment(payment.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(payment);
    }

    [Fact]
    public void GetPayment_NonExistingPayment_ReturnsNull()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = _paymentService.GetPayment(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetPayment_MultiplePayments_ReturnsCorrectPayment()
    {
        // Arrange
        var payment1 = new Payment(Guid.NewGuid(), PaymentStatus.Authorized, "1234", 12, 2025, "USD", 100);
        var payment2 = new Payment(Guid.NewGuid(), PaymentStatus.Declined, "5678", 11, 2024, "EUR", 200);
        _paymentsRepository.Add(payment1);
        _paymentsRepository.Add(payment2);

        // Act
        var result1 = _paymentService.GetPayment(payment1.Id);
        var result2 = _paymentService.GetPayment(payment2.Id);

        // Assert
        result1.Should().BeEquivalentTo(payment1);
        result2.Should().BeEquivalentTo(payment2);
    }

    private static PostPaymentRequest CreateValidPostPaymentRequest()
    {
        return new PostPaymentRequest(
            CardNumber: "1234567890123456",
            ExpiryMonth: 12,
            ExpiryYear: 2025,
            Cvv: "123",
            Currency: "USD",
            Amount: 100);
    }

    private void SetupValidatorSuccess()
    {
        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<PostPaymentRequest>(), default))
            .ReturnsAsync(new ValidationResult());
    }

    private void SetupValidatorFailure()
    {
        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<PostPaymentRequest>(), default))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("CardNumber", "Invalid card number") }));
    }

    private void SetupAcquiringBankClientSuccess()
    {
        _mockAcquiringBankClient.Setup(c => c.ProcessPayment(It.IsAny<AcquiringBankPaymentRequest>()))
            .ReturnsAsync(new AcquiringBankPaymentResult.Success(Authorized: true, AuthorizationCode: "0bb07405-6d44-4b50-a14f-7ae0beff13ad"));
    }

    private void SetupAcquiringBankClientDeclined()
    {
        _mockAcquiringBankClient.Setup(c => c.ProcessPayment(It.IsAny<AcquiringBankPaymentRequest>()))
            .ReturnsAsync(new AcquiringBankPaymentResult.Success(Authorized: false, AuthorizationCode: "0bb07405-6d44-4b50-a14f-7ae0beff13ef"));
    }

    private void SetupAcquiringBankClientError()
    {
        _mockAcquiringBankClient.Setup(c => c.ProcessPayment(It.IsAny<AcquiringBankPaymentRequest>()))
            .ReturnsAsync(new AcquiringBankPaymentResult.Error());
    }
}