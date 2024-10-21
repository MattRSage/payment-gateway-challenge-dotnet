namespace PaymentGateway.Api.Models.Responses;

public record PostPaymentResponse(
    Guid Id,
    string Status,
    string CardNumberLastFour,
    int ExpiryMonth,
    int ExpiryYear,
    string Currency,
    int Amount);