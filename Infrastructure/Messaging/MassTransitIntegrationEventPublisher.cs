using EcommerceBackend.Application.Abstractions.Messaging;
using MassTransit;

namespace EcommerceBackend.Infrastructure.Messaging;

public sealed class MassTransitIntegrationEventPublisher(IPublishEndpoint publishEndpoint) : IIntegrationEventPublisher
{
    public Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : class =>
        publishEndpoint.Publish(integrationEvent, cancellationToken);
}
