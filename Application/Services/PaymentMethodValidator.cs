using System.Text.RegularExpressions;

namespace EcommerceBackend.Application.Services;

/// <summary>
/// Kart kuralları (§4.9): tam kart numarası ve CVV saklanmaz; yalnızca gerçek son 4 haneli maske
/// (<c>**** **** **** 1111</c>) tutulur.
/// </summary>
public static partial class PaymentMethodValidator
{
    /// <summary>Son 4 hanesi bilinmeyen (eski, hash'lenmiş) kayıtların görünümü.</summary>
    public const string UnknownMaskedCardNumber = "**** **** **** ****";

    /// <summary>Kart son kullanma tarihinin geçip geçmediği (UTC ay sonu dahil geçerli).</summary>
    public static bool IsExpired(int expiryMonth, int expiryYear, DateTime utcNow)
    {
        if (expiryMonth is < 1 or > 12 || expiryYear is < 1 or > 9998)
            return true;

        var lastDay = new DateTime(expiryYear, expiryMonth, 1, 0, 0, 0, DateTimeKind.Utc)
            .AddMonths(1)
            .AddDays(-1);
        return utcNow.Date > lastDay.Date;
    }

    /// <summary>Boşluk/tire temizlendikten sonra 12–19 rakamsa rakamları döner, değilse null.</summary>
    public static string? NormalizeCardNumber(string? cardNumber)
    {
        var digits = (cardNumber ?? string.Empty).Replace(" ", string.Empty).Replace("-", string.Empty);
        return CardDigits().IsMatch(digits) ? digits : null;
    }

    public static bool IsMasked(string? cardNumber) => cardNumber?.Contains('*') == true;

    public static string MaskCardNumber(string digits) => $"**** **** **** {digits[^4..]}";

    /// <summary>Saklanan değer maske biçiminde değilse (eski hash kayıtları) son 4 hane gösterilmez.</summary>
    public static string DisplayCardNumber(string? stored) =>
        stored != null && StoredCardMask().IsMatch(stored) ? stored : UnknownMaskedCardNumber;

    /// <summary>Hesap numarası <c>****1234</c> olarak saklanır.</summary>
    public static string? MaskAccountNumber(string? accountNumber)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            return null;

        var compact = accountNumber.Replace(" ", string.Empty).Replace("-", string.Empty);
        return compact.Length < 4 ? "****" : "****" + compact[^4..];
    }

    public static string? DisplayAccountNumber(string? stored) =>
        stored == null ? null : StoredAccountMask().IsMatch(stored) ? stored : "****";

    [GeneratedRegex(@"^\d{12,19}$")]
    private static partial Regex CardDigits();

    [GeneratedRegex(@"^\*{4} \*{4} \*{4} (\d{4}|\*{4})$")]
    private static partial Regex StoredCardMask();

    [GeneratedRegex(@"^\*{4}[A-Za-z0-9]{0,4}$")]
    private static partial Regex StoredAccountMask();
}
