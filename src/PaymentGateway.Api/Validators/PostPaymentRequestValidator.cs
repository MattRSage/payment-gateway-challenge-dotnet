using System.Text.RegularExpressions;

using FluentValidation;

using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Validators;

public class PostPaymentRequestValidator : AbstractValidator<PostPaymentRequest>
{
    private static readonly string[] ValidCurrencies = ["USD", "EUR", "GBP"];

    public PostPaymentRequestValidator()
    {
        RuleFor(x => x.CardNumber)
            .NotEmpty().WithMessage("Card number is required.")
            .Length(14, 19).WithMessage("Card number must be between 14-19 characters long.")
            .Must(BeNumeric).WithMessage("Card number must contain only numeric characters.");

        RuleFor(x => x.ExpiryMonth)
            .NotEmpty().WithMessage("Expiry month is required.")
            .InclusiveBetween(1, 12).WithMessage("Expiry month must be between 1-12.");

        RuleFor(x => x.ExpiryYear)
            .NotEmpty().WithMessage("Expiry year is required.")
            .Must((request, year) => HaveValidExpiryDate(request.ExpiryMonth, year)).WithMessage("Expiry year must be in the future.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be 3 characters long.")
            .Must(BeValidCurrency).WithMessage("Invalid currency code.");

        RuleFor(x => x.Amount)
            .NotEmpty().WithMessage("Amount is required.")
            .GreaterThan(0).WithMessage("Amount must be greater than 0.");

        RuleFor(x => x.Cvv)
            .NotEmpty().WithMessage("CVV is required.")
            .Length(3, 4).WithMessage("CVV must be 3-4 characters long.")
            .Must(BeNumeric).WithMessage("CVV must contain only numeric characters.");
    }

    private static bool BeNumeric(string value) => Regex.IsMatch(value, @"^\d+$");

    private static bool HaveValidExpiryDate(int month, int year)
    {
        try
        {
            var expiryDate = new DateTime(year, month, 1).AddMonths(1).AddDays(-1); // Last day of the month
            return expiryDate > DateTime.UtcNow;
        }
        catch
        {
            return false;
        }
    }

    private bool BeValidCurrency(string currency) => ValidCurrencies.Contains(currency);
}