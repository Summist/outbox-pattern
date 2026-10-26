using Microsoft.EntityFrameworkCore;

using OutboxPattern.Application.Abstractions.Persistence;
using OutboxPattern.Domain.Orders;
using OutboxPattern.Infrastructure.Outbox;

namespace OutboxPattern.Infrastructure.Persistence;

/// <summary>
/// Контекст доступа к базе данных приложения.
/// </summary>
public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options) : DbContext(options), IUnitOfWork
{
    /// <summary>
    /// Заказы покупателей.
    /// </summary>
    public DbSet<Order> Orders => Set<Order>();

    /// <summary>
    /// Сообщения Outbox, ожидающие обработки.
    /// </summary>
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    /// <summary>
    /// Применяет EF-конфигурации, найденные в сборке Infrastructure.
    /// </summary>
    /// <param name="modelBuilder">Построитель модели данных.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);
    }
}
