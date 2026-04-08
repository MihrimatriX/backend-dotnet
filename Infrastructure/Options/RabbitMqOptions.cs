namespace EcommerceBackend.Infrastructure.Options;

public class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    /// <summary>
    /// RabbitMq: gerçek broker; InMemory: yerel geliştirme (Docker gerektirmez).
    /// </summary>
    public string Transport { get; set; } = "InMemory";

    public string Host { get; set; } = "localhost";

    /// <summary>AMQP port (Testcontainers veya özel broker için).</summary>
    public ushort Port { get; set; } = 5672;

    public string VirtualHost { get; set; } = "/";

    public string Username { get; set; } = "guest";

    public string Password { get; set; } = "guest";
}
