using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Infrastructure.AcquiringBank;

public class AcquiringBankPaymentResponse
{
    [JsonPropertyName("authorized")]
    public bool Authorized { get; init; }

    [JsonPropertyName("authorization_code")]
    public string AuthorizationCode { get; init; } = null!;
}