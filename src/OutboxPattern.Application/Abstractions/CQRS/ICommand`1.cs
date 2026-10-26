namespace OutboxPattern.Application.Abstractions.CQRS;

/// <summary>
/// Маркерный интерфейс команды, представляющей намерение выполнить действие
/// с возвращаемым результатом.
/// </summary>
/// <typeparam name="TResponse">
/// Тип результата выполнения команды.
/// </typeparam>
public interface ICommand<TResponse>;
