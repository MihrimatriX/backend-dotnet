namespace EcommerceBackend.Application.Services;

public static class ProductPricing
{
    /// <summary>
    /// Yüzde indirim sonrası birim satış fiyatı (sipariş ve sepet ile uyumlu).
    /// </summary>
    public static decimal EffectiveUnitPrice(decimal unitPrice, int discountPercent)
    {
        var d = Math.Clamp(discountPercent, 0, 100);
        return Math.Round(unitPrice * (1 - d / 100m), 2, MidpointRounding.AwayFromZero);
    }
}
