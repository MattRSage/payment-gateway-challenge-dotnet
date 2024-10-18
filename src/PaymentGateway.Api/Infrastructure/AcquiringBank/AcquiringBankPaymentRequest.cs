using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Infrastructure.AcquiringBank;

public class AcquiringBankPaymentRequest
{
    [JsonPropertyName("card_number")]
    public string CardNumber { get; init; } = null!;
    
    [JsonPropertyName("expiry_date")]
    public string ExpiryDate { get; init; } = null!;
    
    [JsonPropertyName("cvv")]
    public string Cvv { get; init; } = null!;
    
    [JsonPropertyName("currency")]
    public string Currency { get; init; } = null!;
    
    [JsonPropertyName("amount")]
    public int Amount { get; init; }
}