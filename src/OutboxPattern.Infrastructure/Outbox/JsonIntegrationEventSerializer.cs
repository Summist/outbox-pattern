using System.Text.Json;

using OutboxPattern.Application.Abstractions.Outbox;

namespace OutboxPattern.Infrastructure.Outbox;

/// <summary>
/// Сериализует интеграционные события в JSON средствами System.Text.Json.
/// </summary>
public sealed class JsonIntegrationEventSerializer : IIntegrationEventSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web);

    /// <inheritdoc />
    public SerializedIntegrationEvent Serialize(IIntegrationEvent integrationEvent)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        var integrationEventType = integrationEvent.GetType();
        var eventType = integrationEventType.AssemblyQualifiedName
            ?? throw new InvalidOperationException(
                $"Не удалось определить имя типа события {integrationEventType.Name}.");

        var payload = JsonSerializer.Serialize(
            integrationEvent,
            integrationEventType,
            SerializerOptions);

        return new SerializedIntegrationEvent(eventType, payload);
    }

    /// <inheritdoc />
    public IIntegrationEvent Deserialize(string eventType, string payload)
    {
        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new ArgumentException(
                "Тип события обязателен.",
                nameof(eventType));
        }

        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new ArgumentException(
                "Payload события обязателен.",
                nameof(payload));
        }

        var integrationEventType = Type.GetType(eventType, throwOnError: false)
            ?? throw new InvalidOperationException(
                $"Не удалось загрузить тип события {eventType}.");

        if (!typeof(IIntegrationEvent).IsAssignableFrom(integrationEventType))
        {
            throw new InvalidOperationException(
                $"Тип {integrationEventType.FullName} не реализует {nameof(IIntegrationEvent)}.");
        }

        try
        {
            return JsonSerializer.Deserialize(
                payload,
                integrationEventType,
                SerializerOptions) as IIntegrationEvent
                ?? throw new InvalidOperationException(
                    $"Не удалось десериализовать событие типа {integrationEventType.FullName}.");
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException(
                $"Не удалось десериализовать событие типа {integrationEventType.FullName}.",
                exception);
        }
    }
}
