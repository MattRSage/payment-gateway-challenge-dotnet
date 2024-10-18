using PaymentGateway.Api.Domain.Payments;

namespace PaymentGateway.Api.Models.Responses;

public record PostPaymentResponse(
    Guid Id,
    PaymentStatus Status,
    string CardNumberLastFour,
    int ExpiryMonth,
    int ExpiryYear,
    string Currency,
    int Amount);
