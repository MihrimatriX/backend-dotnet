namespace EcommerceBackend.Application.Abstractions.Messaging;

/// <summary>
/// Uygulama katmanından domain entegrasyon olaylarını yayınlamak için soyutlama (RabbitMQ / in-memory vb.).
/// </summary>
public interface IIntegrationEventPublisher
{
    Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : class;
}
