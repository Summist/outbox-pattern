using OutboxPattern.Application.Abstractions.CQRS;

namespace OutboxPattern.Application.UseCases.CreateOrder;

/// <summary>
/// Представляет команду создания заказа. Результатом выполнения является
/// идентификатор созданного заказа.
/// </summary>
public sealed record CreateOrderCommand : ICommand<Guid>
{
    /// <summary>
    /// Адрес электронной почты покупателя.
    /// </summary>
    public required string CustomerEmail { get; init; }

    /// <summary>
    /// Итоговая сумма заказа.
    /// </summary>
    public required decimal TotalPrice { get; init; }
}
