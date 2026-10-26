namespace OutboxPattern.Domain.Orders;

/// <summary>
/// Определяет возможные статусы заказа.
/// </summary>
/// <remarks>
/// Значения кратны 10, чтобы новые статусы можно было добавлять между существующими
/// без изменения уже сохранённых в базе данных значений.
/// </remarks>
public enum OrderStatus
{
    /// <summary>
    /// Заказ создан и ожидает подтверждения или отмены.
    /// </summary>
    Pending = 10,

    /// <summary>
    /// Заказ подтверждён.
    /// </summary>
    Confirmed = 20,

    /// <summary>
    /// Заказ отменён.
    /// </summary>
    Cancelled = 30
}