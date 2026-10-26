
using OutboxPattern.Application.Abstractions.Persistence.Repositories;
using OutboxPattern.Domain.Orders;

namespace OutboxPattern.Infrastructure.Persistence.Repositories;

/// <summary>
/// Репозиторий заказов на основе EF Core.
/// </summary>
public sealed class OrderRepository(
    ApplicationDbContext dbContext) : IOrderRepository
{
    /// <inheritdoc />
    public async Task AddAsync(
        Order order,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Orders.AddAsync(order, cancellationToken);
    }
}
