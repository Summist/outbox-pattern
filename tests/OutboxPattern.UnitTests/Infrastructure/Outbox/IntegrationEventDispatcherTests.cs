using Microsoft.Extensions.DependencyInjection;

using OutboxPattern.Application.Abstractions.Outbox;
using OutboxPattern.Application.Events.Orders;
using OutboxPattern.Infrastructure.Outbox;

namespace OutboxPattern.UnitTests.Infrastructure.Outbox;

public sealed class IntegrationEventDispatcherTests
{
    [Fact]
    public async Task DispatchAsync_RegisteredHandler_InvokesHandler()
    {
        // Arrange
        var handler = new TestOrderCreatedIntegrationEventHandler();
        var services = new ServiceCollection();

        services.AddSingleton<
            IIntegrationEventHandler<OrderCreatedIntegrationEvent>>(handler);

        await using var serviceProvider = services.BuildServiceProvider();

        var dispatcher = new IntegrationEventDispatcher(serviceProvider);
        var integrationEvent = CreateOrderCreatedEvent();

        // Act
        await dispatcher.DispatchAsync(integrationEvent);

        // Assert
        Assert.Same(integrationEvent, handler.HandledEvent);
    }

    [Fact]
    public async Task DispatchAsync_NoHandlers_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        await using var serviceProvider = services.BuildServiceProvider();

        var dispatcher = new IntegrationEventDispatcher(serviceProvider);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => dispatcher.DispatchAsync(CreateOrderCreatedEvent()));
    }

    [Fact]
    public async Task DispatchAsync_HandlerThrows_PropagatesOriginalException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<
            IIntegrationEventHandler<OrderCreatedIntegrationEvent>>(
            new ThrowingOrderCreatedIntegrationEventHandler());

        await using var serviceProvider = services.BuildServiceProvider();

        var dispatcher = new IntegrationEventDispatcher(serviceProvider);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => dispatcher.DispatchAsync(CreateOrderCreatedEvent()));

        Assert.Equal("Ошибка обработчика.", exception.Message);
    }

    private static OrderCreatedIntegrationEvent CreateOrderCreatedEvent()
    {
        return new OrderCreatedIntegrationEvent
        {
            OrderId = Guid.CreateVersion7(),
            CustomerEmail = "customer@example.com",
            TotalPrice = 1500m,
            OccurredAtUtc = DateTimeOffset.UtcNow
        };
    }

    private sealed class TestOrderCreatedIntegrationEventHandler
        : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
    {
        public OrderCreatedIntegrationEvent? HandledEvent { get; private set; }

        public Task HandleAsync(
            OrderCreatedIntegrationEvent integrationEvent,
            CancellationToken cancellationToken = default)
        {
            HandledEvent = integrationEvent;
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingOrderCreatedIntegrationEventHandler
        : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
    {
        public Task HandleAsync(
            OrderCreatedIntegrationEvent integrationEvent,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Ошибка обработчика.");
        }
    }
}
