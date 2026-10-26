using OutboxPattern.Application.Abstractions.Outbox;
using OutboxPattern.Domain.Orders;

namespace OutboxPattern.Application.Events.Orders;

/// <summary>
/// Событие, публикуемое после успешного создания заказа.
/// </summary>
public sealed record OrderCreatedIntegrationEvent : IIntegrationEvent
{
    /// <summary>
    /// Идентификатор созданного заказа.
    /// </summary>
    public required Guid OrderId { get; init; }

    /// <summary>
    /// Адрес электронной почты покупателя.
    /// </summary>
    public required string CustomerEmail { get; init; }

    /// <summary>
    /// Итоговая сумма заказа.
    /// </summary>
    public required decimal TotalPrice { get; init; }

    /// <inheritdoc />
    public required DateTimeOffset OccurredAtUtc { get; init; }

    /// <summary>
    /// Создаёт событие создания заказа по доменной сущности.
    /// </summary>
    /// <param name="order">Созданный заказ.</param>
    /// <returns>Интеграционное событие создания заказа.</returns>
    /// <exception cref="ArgumentNullException">
    /// Возникает, если <paramref name="order"/> равен <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// Время возникновения события берётся из даты создания заказа.
    /// </remarks>
    public static OrderCreatedIntegrationEvent Create(Order order)
    {
        ArgumentNullException.ThrowIfNull(order, nameof(order));

        return new OrderCreatedIntegrationEvent
        {
            OrderId = order.Id,
            CustomerEmail = order.CustomerEmail,
            TotalPrice = order.TotalPrice,
            OccurredAtUtc = order.CreatedAtUtc
        };
    }
}
