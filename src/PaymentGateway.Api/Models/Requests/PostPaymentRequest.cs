namespace PaymentGateway.Api.Models.Requests;

public record PostPaymentRequest(
    string CardNumber,
    int ExpiryMonth,
    int ExpiryYear,
    string Cvv,
    string Currency,
    int Amount);