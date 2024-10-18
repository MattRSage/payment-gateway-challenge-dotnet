namespace PaymentGateway.Api.Domain.Payments;

public record Payment(
    Guid Id,
    PaymentStatus Status,
    string LastFourCardDigits,
    int ExpiryMonth,
    int ExpiryYear,
    string Currency,
    int Amount)
{
    public string? AuthorizationCode { get; init; }
};