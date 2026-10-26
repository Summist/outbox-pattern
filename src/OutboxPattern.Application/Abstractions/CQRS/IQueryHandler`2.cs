namespace OutboxPattern.Application.Abstractions.CQRS;

/// <summary>
/// Определяет обработчик запроса, возвращающего результат.
/// </summary>
/// <typeparam name="TQuery">Тип обрабатываемого запроса.</typeparam>
/// <typeparam name="TResponse">Тип результата выполнения запроса.</typeparam>
public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    /// <summary>
    /// Выполняет запрос и возвращает результат.
    /// </summary>
    /// <param name="query">Запрос для выполнения.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Результат выполнения запроса.</returns>
    Task<TResponse> HandleAsync(
        TQuery query,
        CancellationToken cancellationToken = default);
}
