namespace OutboxPattern.Application.Abstractions.Outbox;

/// <summary>
/// Результат сериализации интеграционного события.
/// </summary>
/// <param name="EventType">Полное имя типа события.</param>
/// <param name="Payload">JSON-представление события.</param>
public sealed record SerializedIntegrationEvent(
    string EventType,
    string Payload);
