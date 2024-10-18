using FluentValidation.Results;

using OneOf;

using PaymentGateway.Api.Domain.Payments;

namespace PaymentGateway.Api.Services;

public class ProcessPaymentResult : OneOfBase<
    ProcessPaymentResult.Success,
    ProcessPaymentResult.Rejected,
    ProcessPaymentResult.Error>
{
    private ProcessPaymentResult(OneOf<Success, Rejected, Error> _) : base(_) { }

    public static implicit operator ProcessPaymentResult(Success _) => new(_);
    
    public static implicit operator ProcessPaymentResult(Rejected _) => new(_);
    
    public static implicit operator ProcessPaymentResult(Error _) => new(_);
    
    public record Success(Payment Payment);
    public record Rejected(ValidationResult ValidationResult);
    public record Error;
}