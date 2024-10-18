namespace PaymentGateway.Api.Infrastructure.AcquiringBank;

public interface IAcquiringBankClient
{
    Task<AcquiringBankPaymentResult> ProcessPayment(AcquiringBankPaymentRequest request);
}