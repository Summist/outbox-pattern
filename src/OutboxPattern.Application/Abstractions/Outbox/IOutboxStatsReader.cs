namespace OutboxPattern.Application.Abstractions.Outbox;

/// <summary>
/// Возвращает статистику по сообщениям Outbox.
/// </summary>
public interface IOutboxStatsReader
{
    /// <summary>
    /// Возвращает текущие счётчики сообщений Outbox.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Статистика Outbox.</returns>
    Task<OutboxStats> GetAsync(
        CancellationToken cancellationToken = default);
}
