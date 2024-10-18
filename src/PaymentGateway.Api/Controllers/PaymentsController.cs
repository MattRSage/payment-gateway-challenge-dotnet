using FluentValidation.AspNetCore;

using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : Controller
{
    private readonly PaymentService _paymentService;

    public PaymentsController(
        PaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost]
    public async Task<ActionResult<PostPaymentResponse>> ProcessPaymentAsync([FromBody] PostPaymentRequest request)
    {
        var result = await _paymentService.ProcessPayment(request);

        return result.Match(
            success => Ok(new PostPaymentResponse(
                success.Payment.Id,
                success.Payment.Status,
                success.Payment.LastFourCardDigits,
                success.Payment.ExpiryMonth,
                success.Payment.ExpiryYear,
                success.Payment.Currency,
                success.Payment.Amount)),
            rejected =>
            {
                rejected.ValidationResult.AddToModelState(ModelState);
                return BadRequest(ModelState);
            },
            error => Problem());
    }

    [HttpGet("{id:guid}")]
    public ActionResult<GetPaymentResponse?> GetPaymentAsync(Guid id)
    {
        var payment = _paymentService.GetPayment(id);

        if (payment is null)
            return NotFound();

        return Ok(new GetPaymentResponse(
            payment.Id,
            payment.Status,
            payment.LastFourCardDigits,
            payment.ExpiryMonth,
            payment.ExpiryYear,
            payment.Currency,
            payment.Amount));
    }
}