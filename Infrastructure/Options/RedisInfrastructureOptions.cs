namespace EcommerceBackend.Infrastructure.Options;

public class RedisInfrastructureOptions
{
    public const string SectionName = "Redis";

    /// <summary>
    /// StackExchange.Redis bağlantı dizesi (örn. localhost:6379 veya şifreli URI).
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// Tüm anahtarlara önek (paylaşımlı Redis ortamında çakışmayı önler).
    /// </summary>
    public string InstanceName { get; set; } = "Ecommerce:";
}
