namespace OutboxPattern.Application.Abstractions.Persistence;

/// <summary>
/// Определяет единицу работы с данными.
/// </summary>
/// <remarks>
/// Атомарно сохраняет изменения агрегатов и накопленные события Outbox в одной транзакции.
/// </remarks>
public interface IUnitOfWork
{
    /// <summary>
    /// Сохраняет изменения.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Количество сохранённых изменений.</returns>
    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
