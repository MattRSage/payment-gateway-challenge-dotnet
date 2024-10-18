using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Domain.Payments;
using PaymentGateway.Api.Infrastructure.Payments;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Tests.IntegrationTests;

public class PaymentsControllerTests
{
    private readonly Random _random = new();

    [Fact]
    public async Task GetPayment_IdExists_ReturnsPayment()
    {
        // Arrange
        var payment = new Payment(
            Id: Guid.NewGuid(),
            Status: PaymentStatus.Authorized,
            ExpiryMonth: _random.Next(1, 12),
            ExpiryYear: _random.Next(2024, 2030),
            Amount: _random.Next(1, 10000),
            LastFourCardDigits: _random.Next(1111, 9999).ToString(),
            Currency: "GBP");

        var paymentsRepository = new PaymentsRepository();
        paymentsRepository.Add(payment);

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => ((ServiceCollection)services)
                .AddSingleton(paymentsRepository)))
            .CreateClient();

        // Act
        var response = await client.GetAsync($"/api/Payments/{payment.Id}");
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
    }

    [Fact]
    public async Task GetPayment_IdDoesNotExist_Returns404NotFound()
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/Payments/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostPayment_ReturnsAuthorizedPayment()
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.CreateClient();

        var request = new PostPaymentRequest(
            CardNumber: "2222405343248877",
            ExpiryMonth: 4,
            ExpiryYear: 2025,
            Cvv: "123",
            Currency: "GBP",
            Amount: 100);

        // Act
        var response = await client.PostAsJsonAsync($"/api/Payments", request);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        paymentResponse!.Id.Should().NotBeEmpty();
        paymentResponse.Status.Should().Be(PaymentStatus.Authorized);
        paymentResponse.CardNumberLastFour.Should().Be(request.CardNumber[^4..]);
        paymentResponse.ExpiryMonth.Should().Be(request.ExpiryMonth);
        paymentResponse.ExpiryYear.Should().Be(request.ExpiryYear);
        paymentResponse.Currency.Should().Be(request.Currency);
        paymentResponse.Amount.Should().Be(request.Amount);
    }

    [Fact]
    public async Task PostPayment_ReturnsDeclinedPayment()
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.CreateClient();

        var request = new PostPaymentRequest(
            CardNumber: "2222405343248112",
            ExpiryMonth: 1,
            ExpiryYear: 2026,
            Cvv: "456",
            Currency: "USD",
            Amount: 60000);

        // Act
        var response = await client.PostAsJsonAsync($"/api/Payments", request);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        paymentResponse!.Id.Should().NotBeEmpty();
        paymentResponse.Status.Should().Be(PaymentStatus.Declined);
        paymentResponse.CardNumberLastFour.Should().Be(request.CardNumber[^4..]);
        paymentResponse.ExpiryMonth.Should().Be(request.ExpiryMonth);
        paymentResponse.ExpiryYear.Should().Be(request.ExpiryYear);
        paymentResponse.Currency.Should().Be(request.Currency);
        paymentResponse.Amount.Should().Be(request.Amount);
    }

    [Fact]
    public async Task PostPayment_ReturnsRejected()
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.CreateClient();

        var request = new PostPaymentRequest(
            CardNumber: "222240",
            ExpiryMonth: 13,
            ExpiryYear: 2023,
            Cvv: "45",
            Currency: "AUD",
            Amount: 0);

        // Act
        var response = await client.PostAsJsonAsync($"/api/Payments", request);
        var paymentResponse = await response.Content.ReadFromJsonAsync<Dictionary<string, string[]>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        paymentResponse.Should().ContainKey("CardNumber");
        paymentResponse!["CardNumber"].Should().Contain("Card number must be between 14-19 characters long.");

        paymentResponse.Should().ContainKey("ExpiryMonth");
        paymentResponse["ExpiryMonth"].Should().Contain("Expiry month must be between 1-12.");

        paymentResponse.Should().ContainKey("ExpiryYear");
        paymentResponse["ExpiryYear"].Should().Contain("Expiry year must be in the future.");

        paymentResponse.Should().ContainKey("Cvv");
        paymentResponse["Cvv"].Should().Contain("CVV must be 3-4 characters long.");

        paymentResponse.Should().ContainKey("Currency");
        paymentResponse["Currency"].Should().Contain("Invalid currency code.");

        paymentResponse.Should().ContainKey("Amount");
        paymentResponse["Amount"].Should().Contain("Amount must be greater than 0.");
    }

    [Theory]
    [InlineData("2222405343248877", 4, 2025, "123", "GBP", 100, PaymentStatus.Authorized)] // Authorized payment
    [InlineData("2222405343248112", 1, 2026, "456", "USD", 60000, PaymentStatus.Declined)] // Declined payment
    public async Task PostPayment_ThenRetrieve_ReturnsPayment(
        string cardNumber,
        int expiryMonth,
        int expiryYear,
        string cvv,
        string currency,
        int amount,
        PaymentStatus paymentStatus)
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.CreateClient();

        var request = new PostPaymentRequest(
            CardNumber: cardNumber,
            ExpiryMonth: expiryMonth,
            ExpiryYear: expiryYear,
            Cvv: cvv,
            Currency: currency,
            Amount: amount);

        var postResponse = await client.PostAsJsonAsync($"/api/Payments", request);
        var paymentPostResponse = await postResponse.Content.ReadFromJsonAsync<PostPaymentResponse>();

        // Act
        var response = await client.GetAsync($"/api/Payments/{paymentPostResponse!.Id}");
        var paymentResponse = await response.Content.ReadFromJsonAsync<GetPaymentResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        paymentResponse!.Id.Should().Be(paymentPostResponse.Id);
        paymentResponse.Status.Should().Be(paymentStatus);
        paymentResponse.CardNumberLastFour.Should().Be(request.CardNumber[^4..]);
        paymentResponse.ExpiryMonth.Should().Be(request.ExpiryMonth);
        paymentResponse.ExpiryYear.Should().Be(request.ExpiryYear);
        paymentResponse.Currency.Should().Be(request.Currency);
        paymentResponse.Amount.Should().Be(request.Amount);
    }

    [Fact]
    public async Task PostPayment_AcquiringBankNotAvailable_Returns500Error()
    {
        // Arrange
        var request = new PostPaymentRequest(
            CardNumber: "2222405343248877",
            ExpiryMonth: 4,
            ExpiryYear: 2025,
            Cvv: "123",
            Currency: "GBP",
            Amount: 100);

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
                builder.ConfigureServices((context, services) =>
                {
                    context.Configuration["AcquiringBank:BaseUri"] = "https://localhost:8080/random";
                }))
            .CreateClient();

        // Act
        var response = await client.PostAsJsonAsync($"/api/Payments", request);
        var responseString = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
}