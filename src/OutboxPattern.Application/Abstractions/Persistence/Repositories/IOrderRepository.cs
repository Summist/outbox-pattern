using OutboxPattern.Domain.Orders;

namespace OutboxPattern.Application.Abstractions.Persistence.Repositories;

/// <summary>
/// Определяет операции хранения заказов.
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Добавляет заказ в хранилище в рамках текущей транзакции.
    /// </summary>
    /// <remarks>
    /// Заказ сохраняется при следующем вызове <see cref="IUnitOfWork.SaveChangesAsync"/>.
    /// </remarks>
    /// <param name="order">Заказ для сохранения.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    Task AddAsync(
        Order order,
        CancellationToken cancellationToken = default);
}
