using FluentValidation;

using PaymentGateway.Api.Infrastructure.AcquiringBank;
using PaymentGateway.Api.Infrastructure.Payments;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<PaymentService>();
builder.Services.AddSingleton<PaymentsRepository>();
builder.Services.AddSingleton<IValidator<PostPaymentRequest>, PostPaymentRequestValidator>();

builder.Services.AddHttpClient<IAcquiringBankClient, AcquiringBankClient>()
    .ConfigureHttpClient(x => x.BaseAddress = new Uri(builder.Configuration["AcquiringBank:BaseUri"]!));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
