namespace EcommerceBackend.Application.Options;

/// <summary>
/// Checkout ve kargo kuralları (TRY). Eşik altında sabit kargo, üstünde ücretsiz.
/// </summary>
public class CheckoutOptions
{
    public const string SectionName = "Checkout";

    /// <summary>Ara toplam bu tutarın altındaysa kargo uygulanır (TRY).</summary>
    public decimal FreeShippingThresholdTry { get; set; } = 150m;

    /// <summary>Standart kargo ücreti (TRY).</summary>
    public decimal StandardShippingFeeTry { get; set; } = 34.99m;

    /// <summary>Geliştirme ortamında ödeme simülasyonunun reddetme olasılığı (0–100). Prod’da 0 bırakın.</summary>
    public int SimulatedPaymentDeclinePercent { get; set; } = 0;

    /// <summary>
    /// Müşterinin kendi siparişini <c>Pending→Processing→Shipped→Delivered</c> ilerletebildiği demo lojistik ucu (§5.5).
    /// Geliştirmede açık, üretimde kapalı.
    /// </summary>
    public bool DemoFulfillmentEnabled { get; set; } = false;
}
