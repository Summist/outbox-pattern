namespace OutboxPattern.Application.Abstractions.Outbox;

/// <summary>
/// Передаёт интеграционное событие зарегистрированным обработчикам.
/// </summary>
public interface IIntegrationEventDispatcher
{
    /// <summary>
    /// Выполняет обработку интеграционного события.
    /// </summary>
    /// <param name="integrationEvent">Событие для обработки.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    Task DispatchAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default);
}
