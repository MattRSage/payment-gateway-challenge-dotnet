using System.Text.Json;

using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Infrastructure.AcquiringBank;

public class AcquiringBankClient : IAcquiringBankClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AcquiringBankClient> _logger;

    public AcquiringBankClient(HttpClient httpClient, ILogger<AcquiringBankClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AcquiringBankPaymentResult> ProcessPayment(AcquiringBankPaymentRequest request)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync("/payments", request, new JsonSerializerOptions());
            
            response.EnsureSuccessStatusCode();
            
            var acquiringBankPaymentResponse = await response.Content.ReadFromJsonAsync<AcquiringBankPaymentResponse>();

            return acquiringBankPaymentResponse is null 
                ? new AcquiringBankPaymentResult.Error() 
                : new AcquiringBankPaymentResult.Success(
                    acquiringBankPaymentResponse.Authorized,
                    acquiringBankPaymentResponse.AuthorizationCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process payment");
            throw;
        }
    }
}