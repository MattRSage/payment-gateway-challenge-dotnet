using PaymentGateway.Api.Domain.Payments;

namespace PaymentGateway.Api.Infrastructure.Payments;

public class PaymentsRepository : IPaymentRepository
{
    public List<Payment> Payments = new();

    public void Add(Payment payment)
    {
        Payments.Add(payment);
    }

    public Payment? Get(Guid id)
    {
        return Payments.FirstOrDefault(p => p.Id == id);
    }
}