using EcommerceBackend.Application.Options;

namespace EcommerceBackend.Application.Services;

/// <summary>Kargo kuralı (§5.1): ara toplam ≥ eşik → ücretsiz; boş sepet (ara toplam 0) → kargo yok.</summary>
public static class ShippingQuote
{
    public static (decimal Subtotal, decimal ShippingFee, decimal GrandTotal) Calculate(
        decimal cartSubtotal,
        CheckoutOptions options)
    {
        var subtotal = Math.Round(cartSubtotal, 2);
        var threshold = Math.Max(0, options.FreeShippingThresholdTry);
        var fee = subtotal <= 0 || subtotal >= threshold ? 0m : Math.Max(0, options.StandardShippingFeeTry);
        return (subtotal, fee, Math.Round(subtotal + fee, 2));
    }

    /// <summary>Ücretsiz kargo için kalan tutar; eşik aşıldıysa null.</summary>
    public static decimal? FreeShippingRemaining(decimal subtotal, CheckoutOptions options)
    {
        var threshold = Math.Max(0, options.FreeShippingThresholdTry);
        if (subtotal >= threshold)
            return null;
        return Math.Round(threshold - subtotal, 2);
    }
}
