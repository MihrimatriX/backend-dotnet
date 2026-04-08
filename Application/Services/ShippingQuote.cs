using EcommerceBackend.Application.Options;

namespace EcommerceBackend.Application.Services;

public static class ShippingQuote
{
    public static (decimal Subtotal, decimal ShippingFee, decimal GrandTotal) Calculate(
        decimal cartSubtotal,
        CheckoutOptions options)
    {
        var threshold = Math.Max(0, options.FreeShippingThresholdTry);
        var fee = cartSubtotal >= threshold ? 0m : Math.Max(0, options.StandardShippingFeeTry);
        var grand = Math.Round(cartSubtotal + fee, 2);
        return (Math.Round(cartSubtotal, 2), fee, grand);
    }

    public static decimal? FreeShippingRemaining(decimal subtotal, CheckoutOptions options)
    {
        var threshold = Math.Max(0, options.FreeShippingThresholdTry);
        if (subtotal >= threshold)
            return null;
        return Math.Round(threshold - subtotal, 2);
    }
}
