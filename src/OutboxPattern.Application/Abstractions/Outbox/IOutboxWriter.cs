namespace OutboxPattern.Application.Abstractions.Outbox;

/// <summary>
/// Определяет механизм добавления интеграционных событий в Outbox.
/// </summary>
/// <remarks>
/// Событие сохраняется только при вызове <see cref="IUnitOfWork.SaveChangesAsync"/>
/// и должно попадать в ту же транзакцию, что и изменения агрегатов.
/// </remarks>
public interface IOutboxWriter
{
    /// <summary>
    /// Добавляет интеграционное событие для последующей обработки.
    /// </summary>
    /// <param name="integrationEvent">Интеграционное событие для сохранения.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    Task EnqueueAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default);
}
