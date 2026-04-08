using EcommerceBackend.Application.Options;
using EcommerceBackend.Infrastructure.Repositories;
using Microsoft.Extensions.Options;

namespace EcommerceBackend.Application.Services;

public interface ICheckoutPaymentSimulator
{
    /// <summary>Ödeme yetkilendirmesi (demo: son kullanma + isteğe bağlı simüle ret).</summary>
    Task<PaymentAuthorizationResult> AuthorizeAsync(
        int userId,
        int paymentMethodId,
        decimal amountTry,
        CancellationToken ct = default);
}

public sealed record PaymentAuthorizationResult(bool Approved, string MessageTr);

public sealed class CheckoutPaymentSimulator : ICheckoutPaymentSimulator
{
    private readonly CheckoutOptions _options;
    private readonly IPaymentMethodRepository _payments;
    private readonly ILogger<CheckoutPaymentSimulator> _logger;

    public CheckoutPaymentSimulator(
        IOptions<CheckoutOptions> options,
        IPaymentMethodRepository payments,
        ILogger<CheckoutPaymentSimulator> logger)
    {
        _options = options.Value;
        _payments = payments;
        _logger = logger;
    }

    public async Task<PaymentAuthorizationResult> AuthorizeAsync(
        int userId,
        int paymentMethodId,
        decimal amountTry,
        CancellationToken ct = default)
    {
        var pm = await _payments.GetByIdAsync(paymentMethodId);
        if (pm == null || pm.UserId != userId)
            return new PaymentAuthorizationResult(false, "Ödeme yöntemi bulunamadı veya size ait değil.");

        if (PaymentMethodValidator.IsExpired(pm.ExpiryMonth, pm.ExpiryYear, DateTime.UtcNow))
            return new PaymentAuthorizationResult(false, "Kartın son kullanma tarihi geçmiş. Lütfen başka bir kart seçin.");

        var rate = Math.Clamp(_options.SimulatedPaymentDeclinePercent, 0, 100);
        if (rate > 0 && Random.Shared.Next(100) < rate)
        {
            _logger.LogWarning("Simüle ödeme reddi (Checkout:SimulatedPaymentDeclinePercent={Rate})", rate);
            return new PaymentAuthorizationResult(
                false,
                "Ödeme sağlayıcı işlemi onaylamadı. Bankanızı arayın veya farklı kart deneyin.");
        }

        return new PaymentAuthorizationResult(true, "Ödeme yetkisi alındı.");
    }
}
