using PaymentGateway.Api.Domain.Payments;

namespace PaymentGateway.Api.Mapping;

public static class Mapping
{
    public static string ToApi(this PaymentStatus status)
        => status switch
        {
            PaymentStatus.Authorized => "Authorized",
            PaymentStatus.Declined => "Declined",
            PaymentStatus.Rejected => "Rejected",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
}