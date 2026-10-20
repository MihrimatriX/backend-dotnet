namespace EcommerceBackend.Application.Common;

/// <summary>
/// Zarftaki makine kodları (<c>errorCode</c>) — docs/API_CONTRACT.md §1.2 ve uç tabloları.
/// Spring ikiziyle birebir aynı değerler kullanılır.
/// </summary>
public static class ErrorCodes
{
    // Çerçeve / ortak
    public const string ValidationError = "VALIDATION_ERROR";
    public const string BadRequest = "BAD_REQUEST";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string Forbidden = "FORBIDDEN";
    public const string NotFound = "NOT_FOUND";
    public const string MethodNotAllowed = "METHOD_NOT_ALLOWED";
    public const string UnsupportedMediaType = "UNSUPPORTED_MEDIA_TYPE";
    public const string Conflict = "CONFLICT";
    public const string RateLimited = "RATE_LIMITED";
    public const string InternalError = "INTERNAL_ERROR";
    public const string RequestCancelled = "REQUEST_CANCELLED";

    // Kaynak bulunamadı
    public const string ProductNotFound = "PRODUCT_NOT_FOUND";
    public const string CategoryNotFound = "CATEGORY_NOT_FOUND";
    public const string SubCategoryNotFound = "SUBCATEGORY_NOT_FOUND";
    public const string CampaignNotFound = "CAMPAIGN_NOT_FOUND";
    public const string AddressNotFound = "ADDRESS_NOT_FOUND";
    public const string PaymentMethodNotFound = "PAYMENT_METHOD_NOT_FOUND";
    public const string OrderNotFound = "ORDER_NOT_FOUND";
    public const string ReviewNotFound = "REVIEW_NOT_FOUND";
    public const string NotificationNotFound = "NOTIFICATION_NOT_FOUND";
    public const string ArticleNotFound = "ARTICLE_NOT_FOUND";
    public const string UserNotFound = "USER_NOT_FOUND";

    // Kimlik ve hesap
    public const string EmailTaken = "EMAIL_TAKEN";
    public const string InvalidPassword = "INVALID_PASSWORD";
    public const string InvalidImport = "INVALID_IMPORT";

    // Katalog
    public const string CategoryExists = "CATEGORY_EXISTS";
    public const string SubCategoryExists = "SUBCATEGORY_EXISTS";
    public const string IdMismatch = "ID_MISMATCH";
    public const string InvalidDateRange = "INVALID_DATE_RANGE";

    // Sepet ve favori
    public const string InvalidQuantity = "INVALID_QUANTITY";
    public const string OutOfStock = "OUT_OF_STOCK";
    public const string InsufficientStock = "INSUFFICIENT_STOCK";
    public const string NotInCart = "NOT_IN_CART";
    public const string AlreadyFavorite = "ALREADY_FAVORITE";
    public const string NotFavorite = "NOT_FAVORITE";

    // Ödeme yöntemi
    public const string InvalidCardNumber = "INVALID_CARD_NUMBER";
    public const string CardExpired = "CARD_EXPIRED";

    // Sipariş
    public const string IdempotencyKeyInvalid = "IDEMPOTENCY_KEY_INVALID";
    public const string IdempotencyConflict = "IDEMPOTENCY_CONFLICT";
    public const string EmptyOrder = "EMPTY_ORDER";
    public const string InvalidAddress = "INVALID_ADDRESS";
    public const string InvalidPayment = "INVALID_PAYMENT";
    public const string CartUnavailable = "CART_UNAVAILABLE";
    public const string CartMismatch = "CART_MISMATCH";
    public const string CheckoutFailed = "CHECKOUT_FAILED";
    public const string CancelNotAllowed = "CANCEL_NOT_ALLOWED";
    public const string ReturnNotAllowed = "RETURN_NOT_ALLOWED";
    public const string DemoFulfillmentDisabled = "DEMO_FULFILLMENT_DISABLED";
    public const string DemoAdvanceInvalidState = "DEMO_ADVANCE_INVALID_STATE";
    public const string InvalidStatus = "INVALID_STATUS";

    // Yorum
    public const string ReviewExists = "REVIEW_EXISTS";
}

/// <summary>Çerçeve seviyesindeki hatalarda kullanılan sabit metinler (§1.2).</summary>
public static class ErrorMessages
{
    public const string BadRequest = "Geçersiz istek.";
    public const string Unauthorized = "Kimlik doğrulama gerekli.";
    public const string Forbidden = "Bu işlem için yetkiniz yok.";
    public const string NotFound = "Kaynak bulunamadı.";
    public const string MethodNotAllowed = "Bu HTTP metodu bu uç için desteklenmiyor.";
    public const string UnsupportedMediaType = "Desteklenmeyen içerik türü.";
    public const string Conflict = "Kayıt başka bir işlem tarafından değiştirildi. Tekrar deneyin.";
    public const string RateLimited = "Çok fazla istek. Lütfen biraz sonra tekrar deneyin.";
    public const string InternalError = "Beklenmeyen bir hata oluştu. Destek için traceId değerini iletin.";
}
