namespace EcommerceBackend.Infrastructure.Logging;

public static class CorrelationIdConstants
{
    public const string HeaderName = "X-Correlation-Id";
    public const string HttpContextItemKey = "CorrelationId";
}
