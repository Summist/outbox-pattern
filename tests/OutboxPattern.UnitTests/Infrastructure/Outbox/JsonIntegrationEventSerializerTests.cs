using System.Text.Json;

using OutboxPattern.Application.Events.Orders;
using OutboxPattern.Infrastructure.Outbox;

namespace OutboxPattern.UnitTests.Infrastructure.Outbox;

public sealed class JsonIntegrationEventSerializerTests
{
    private readonly JsonIntegrationEventSerializer _serializer = new();

    [Fact]
    public void Serialize_OrderCreatedEvent_ContainsEventData()
    {
        // Arrange
        var integrationEvent = CreateOrderCreatedEvent();

        // Act
        var serialized = _serializer.Serialize(integrationEvent);

        // Assert
        Assert.Equal(
            typeof(OrderCreatedIntegrationEvent).AssemblyQualifiedName,
            serialized.EventType);

        using var document = JsonDocument.Parse(serialized.Payload);
        var payload = document.RootElement;

        Assert.Equal(
            integrationEvent.OrderId,
            payload.GetProperty("orderId").GetGuid());
        Assert.Equal(
            integrationEvent.CustomerEmail,
            payload.GetProperty("customerEmail").GetString());
        Assert.Equal(
            integrationEvent.TotalPrice,
            payload.GetProperty("totalPrice").GetDecimal());
        Assert.Equal(
            integrationEvent.OccurredAtUtc,
            payload.GetProperty("occurredAtUtc").GetDateTimeOffset());
    }

    [Fact]
    public void SerializeAndDeserialize_OrderCreatedEvent_RoundTrips()
    {
        // Arrange
        var integrationEvent = CreateOrderCreatedEvent();

        // Act
        var serialized = _serializer.Serialize(integrationEvent);
        var restored = Assert.IsType<OrderCreatedIntegrationEvent>(
            _serializer.Deserialize(serialized.EventType, serialized.Payload));

        // Assert
        Assert.Equal(integrationEvent.OrderId, restored.OrderId);
        Assert.Equal(integrationEvent.CustomerEmail, restored.CustomerEmail);
        Assert.Equal(integrationEvent.TotalPrice, restored.TotalPrice);
        Assert.Equal(integrationEvent.OccurredAtUtc, restored.OccurredAtUtc);
    }

    [Fact]
    public void Serialize_NullEvent_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => _serializer.Serialize(null!));
    }

    [Theory]
    [InlineData("", "{}")]
    [InlineData(" ", "{}")]
    [InlineData("type", "")]
    public void Deserialize_InvalidArguments_ThrowsArgumentException(
        string eventType,
        string payload)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => _serializer.Deserialize(eventType, payload));
    }

    [Fact]
    public void Deserialize_UnknownEventType_ThrowsInvalidOperationException()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => _serializer.Deserialize(
                "Unknown.Event, Unknown.Assembly",
                "{}"));
    }

    [Fact]
    public void Deserialize_InvalidPayload_ThrowsInvalidOperationException()
    {
        // Arrange
        var eventType = typeof(OrderCreatedIntegrationEvent).AssemblyQualifiedName!;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => _serializer.Deserialize(eventType, "{"));
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
}
