namespace OutboxPattern.Application.Abstractions.Outbox;

/// <summary>
/// Обрабатывает интеграционное событие указанного типа.
/// </summary>
/// <typeparam name="TEvent">Тип обрабатываемого события.</typeparam>
public interface IIntegrationEventHandler<in TEvent>
    where TEvent : IIntegrationEvent
{
    /// <summary>
    /// Обрабатывает событие.
    /// </summary>
    /// <param name="integrationEvent">Событие для обработки.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    Task HandleAsync(
        TEvent integrationEvent,
        CancellationToken cancellationToken = default);
}
