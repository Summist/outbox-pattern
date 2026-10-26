namespace OutboxPattern.Api.Contracts;

/// <summary>
/// Запрос на создание заказа.
/// </summary>
/// <param name="CustomerEmail">Адрес электронной почты покупателя.</param>
/// <param name="TotalPrice">Итоговая сумма заказа.</param>
public sealed record CreateOrderRequest(
    string CustomerEmail,
    decimal TotalPrice);
