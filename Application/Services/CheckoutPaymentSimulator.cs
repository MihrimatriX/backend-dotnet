using EcommerceBackend.Application.Options;
using EcommerceBackend.Domain.Entities;
using Microsoft.Extensions.Options;

namespace EcommerceBackend.Application.Services;

public interface ICheckoutPaymentSimulator
{
    /// <summary>Ödeme yetkilendirmesi (demo: son kullanma + isteğe bağlı simüle ret).</summary>
    Task<PaymentAuthorizationResult> AuthorizeAsync(
        PaymentMethod paymentMethod,
        decimal amountTry,
        CancellationToken ct = default);
}

public sealed record PaymentAuthorizationResult(bool Approved, string MessageTr);

public sealed class CheckoutPaymentSimulator : ICheckoutPaymentSimulator
{
    private readonly CheckoutOptions _options;
    private readonly ILogger<CheckoutPaymentSimulator> _logger;

    public CheckoutPaymentSimulator(IOptions<CheckoutOptions> options, ILogger<CheckoutPaymentSimulator> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task<PaymentAuthorizationResult> AuthorizeAsync(
        PaymentMethod paymentMethod,
        decimal amountTry,
        CancellationToken ct = default)
    {
        if (PaymentMethodValidator.IsExpired(paymentMethod.ExpiryMonth, paymentMethod.ExpiryYear, DateTime.UtcNow))
            return Task.FromResult(new PaymentAuthorizationResult(false, "Kartın son kullanma tarihi geçmiş. Lütfen başka bir kart seçin."));

        var rate = Math.Clamp(_options.SimulatedPaymentDeclinePercent, 0, 100);
        if (rate > 0 && Random.Shared.Next(100) < rate)
        {
            _logger.LogWarning("Simüle ödeme reddi (Checkout:SimulatedPaymentDeclinePercent={Rate})", rate);
            return Task.FromResult(new PaymentAuthorizationResult(
                false,
                "Ödeme sağlayıcı işlemi onaylamadı. Bankanızı arayın veya farklı kart deneyin."));
        }

        return Task.FromResult(new PaymentAuthorizationResult(true, "Ödeme yetkisi alındı."));
    }
}
