using PaymentGateway.Api.Domain.Payments;

namespace PaymentGateway.Api.Infrastructure.Payments;

public interface IPaymentRepository
{
    void Add(Payment payment);
    Payment? Get(Guid id);
}