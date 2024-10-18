using PaymentGateway.Api.Domain.Payments;

namespace PaymentGateway.Api.Models.Responses;

public record GetPaymentResponse(
    Guid Id,
    PaymentStatus Status,
    string CardNumberLastFour,
    int ExpiryMonth,
    int ExpiryYear,
    string Currency,
    int Amount);