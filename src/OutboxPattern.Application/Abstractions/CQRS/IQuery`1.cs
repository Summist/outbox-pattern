namespace OutboxPattern.Application.Abstractions.CQRS;

/// <summary>
/// Представляет запрос на получение данных с результатом заданного типа.
/// </summary>
/// <typeparam name="TResponse">Тип результата выполнения запроса.</typeparam>
public interface IQuery<TResponse>;
