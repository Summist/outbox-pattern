namespace OutboxPattern.Application.Abstractions.CQRS;

/// <summary>
/// Определяет обработчик команды, возвращающей результат.
/// </summary>
/// <typeparam name="TCommand">Тип обрабатываемой команды.</typeparam>
/// <typeparam name="TResponse">Тип результата выполнения команды.</typeparam>
public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    /// <summary>
    /// Выполняет команду и возвращает результат.
    /// </summary>
    /// <param name="command">Команда для выполнения.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Результат выполнения команды.</returns>
    Task<TResponse> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default);
}
