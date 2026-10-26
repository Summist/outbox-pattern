namespace OutboxPattern.Application.Abstractions.Outbox;

/// <summary>
/// Сериализует интеграционные события для хранения в Outbox
/// и восстанавливает их обратно.
/// </summary>
public interface IIntegrationEventSerializer
{
    /// <summary>
    /// Сериализует событие и определяет имя его типа.
    /// </summary>
    /// <param name="integrationEvent">Событие для сериализации.</param>
    /// <returns>Тип события и его JSON-представление.</returns>
    /// <exception cref="ArgumentNullException">
    /// Возникает, если <paramref name="integrationEvent"/> равен <see langword="null"/>.
    /// </exception>
    SerializedIntegrationEvent Serialize(IIntegrationEvent integrationEvent);

    /// <summary>
    /// Восстанавливает событие по имени типа и JSON-представлению.
    /// </summary>
    /// <param name="eventType">
    /// Полное имя типа события, достаточное для его загрузки.
    /// </param>
    /// <param name="payload">JSON-представление события.</param>
    /// <returns>Восстановленное интеграционное событие.</returns>
    /// <exception cref="ArgumentException">
    /// Возникает, если имя типа или payload пуст.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Возникает, если тип не найден, не реализует <see cref="IIntegrationEvent"/>
    /// или payload не удалось десериализовать.
    /// </exception>
    IIntegrationEvent Deserialize(string eventType, string payload);
}
