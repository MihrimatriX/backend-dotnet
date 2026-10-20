namespace EcommerceBackend.Application.Orders;

/// <summary>Sipariş durumları (§1, §5.4): düz string, kanonik yazımla saklanır.</summary>
public static class OrderStatuses
{
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string Shipped = "Shipped";
    public const string Delivered = "Delivered";
    public const string Cancelled = "Cancelled";
    public const string ReturnRequested = "ReturnRequested";
    public const string Returned = "Returned";

    public static readonly IReadOnlyList<string> All =
        [Pending, Processing, Shipped, Delivered, Cancelled, ReturnRequested, Returned];

    /// <summary>Harf duyarsız eşleşmede kanonik yazımı döner.</summary>
    public static bool TryNormalize(string? value, out string canonical)
    {
        canonical = All.FirstOrDefault(s => string.Equals(s, value?.Trim(), StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
        return canonical.Length > 0;
    }

    public static bool IsCustomerCancellable(string status) => status is Pending or Processing;

    /// <summary>Demo lojistikte bir sonraki adım (§5.5); yoksa null.</summary>
    public static string? NextDemoStep(string status) => status switch
    {
        Pending => Processing,
        Processing => Shipped,
        Shipped => Delivered,
        _ => null,
    };

    /// <summary>
    /// Stok iadesi: <c>Pending/Processing → Cancelled</c> ve <c>Returned</c>'a geçiş (iptal edilmiş sipariş hariç;
    /// stoğu zaten iade edilmiştir).
    /// </summary>
    public static bool RestoresStock(string previous, string next) =>
        (next == Cancelled && IsCustomerCancellable(previous))
        || (next == Returned && previous != Cancelled && previous != Returned);
}
