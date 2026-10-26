using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using OutboxPattern.Domain.Orders;

namespace OutboxPattern.Infrastructure.Persistence.Configurations;

/// <summary>
/// Конфигурация хранения заказов.
/// </summary>
public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    /// <summary>
    /// Настраивает таблицу заказов.
    /// </summary>
    /// <param name="builder">Построитель конфигурации сущности.</param>
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(order => order.Id);

        builder.Property(order => order.CustomerEmail)
            .HasMaxLength(Order.CustomerEmailMaxLength)
            .IsRequired();

        builder.Property(order => order.TotalPrice)
            .HasPrecision(18, 2);

        builder.Property(order => order.Status)
            .HasConversion<int>()
            .IsRequired();
    }
}
