namespace OutboxPattern.Api.Contracts;

/// <summary>
/// Результат создания заказа.
/// </summary>
/// <param name="OrderId">Идентификатор созданного заказа.</param>
public sealed record CreateOrderResponse(Guid OrderId);
