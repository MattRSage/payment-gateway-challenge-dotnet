using FluentValidation;

using PaymentGateway.Api.Domain.Payments;
using PaymentGateway.Api.Infrastructure.AcquiringBank;
using PaymentGateway.Api.Infrastructure.Payments;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Services;

public class PaymentService
{
    private readonly IValidator<PostPaymentRequest> _validator;
    private readonly IAcquiringBankClient _acquiringBankClient;
    private readonly PaymentsRepository _paymentRepository;

    public PaymentService(
        IAcquiringBankClient acquiringBankClient,
        PaymentsRepository paymentRepository,
        IValidator<PostPaymentRequest> validator)
    {
        _acquiringBankClient = acquiringBankClient;
        _paymentRepository = paymentRepository;
        _validator = validator;
    }

    public async Task<ProcessPaymentResult> ProcessPayment(PostPaymentRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var payment = CreatePayment(
                request,
                PaymentStatus.Rejected);

            _paymentRepository.Add(payment);

            return new ProcessPaymentResult.Rejected(validationResult);
        }

        var paymentRequest = new AcquiringBankPaymentRequest
        {
            CardNumber = request.CardNumber,
            ExpiryDate = $"{request.ExpiryMonth:D2}/{request.ExpiryYear}",
            Cvv = request.Cvv,
            Currency = request.Currency,
            Amount = request.Amount
        };

        var result = await _acquiringBankClient.ProcessPayment(paymentRequest);

        return result.Match<ProcessPaymentResult>(
            success =>
            {
                var payment = CreatePayment(
                    request,
                    success.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined,
                    success.AuthorizationCode);

                _paymentRepository.Add(payment);

                return new ProcessPaymentResult.Success(payment);
            },
            error => new ProcessPaymentResult.Error());
    }

    public Payment? GetPayment(Guid paymentId)
    {
        return _paymentRepository.Get(paymentId);
    }

    private static Payment CreatePayment(PostPaymentRequest request, PaymentStatus status, string? authorizationCode = null)
    => new(
        Guid.NewGuid(),
        status,
        request.CardNumber[^4..],
        request.ExpiryMonth,
        request.ExpiryYear,
        request.Currency,
        request.Amount)
    {
        AuthorizationCode = authorizationCode
    };
}