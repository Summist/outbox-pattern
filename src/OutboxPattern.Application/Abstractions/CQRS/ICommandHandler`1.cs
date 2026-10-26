namespace OutboxPattern.Application.Abstractions.CQRS;

/// <summary>
/// Определяет обработчик команды, не возвращающей результат.
/// </summary>
/// <typeparam name="TCommand">Тип обрабатываемой команды.</typeparam>
public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    /// <summary>
    /// Выполняет команду.
    /// </summary>
    /// <param name="command">Команда для выполнения.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    Task HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default);
}
