namespace EcommerceBackend.Application.Services;

public static class PaymentMethodValidator
{
    /// <summary>Kart son kullanma tarihinin geçerli olduğunu kontrol eder (UTC ay sonu).</summary>
    public static bool IsExpired(int expiryMonth, int expiryYear, DateTime utcNow)
    {
        if (expiryMonth is < 1 or > 12)
            return true;

        var lastDay = new DateTime(expiryYear, expiryMonth, 1, 0, 0, 0, DateTimeKind.Utc)
            .AddMonths(1)
            .AddDays(-1);
        return utcNow.Date > lastDay.Date;
    }
}
