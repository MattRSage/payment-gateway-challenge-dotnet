using OneOf;

namespace PaymentGateway.Api.Infrastructure.AcquiringBank;

public class AcquiringBankPaymentResult : OneOfBase<
    AcquiringBankPaymentResult.Success,
    AcquiringBankPaymentResult.Error>
{
    private AcquiringBankPaymentResult(OneOf<Success, Error> _) : base(_) { }

    public static implicit operator AcquiringBankPaymentResult(Success _) => new(_);

    public static implicit operator AcquiringBankPaymentResult(Error _) => new(_);

    public record Success(bool Authorized, string AuthorizationCode);
    public record Error;
}