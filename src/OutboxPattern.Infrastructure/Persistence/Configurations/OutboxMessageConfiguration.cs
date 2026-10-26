using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using OutboxPattern.Infrastructure.Outbox;

namespace OutboxPattern.Infrastructure.Persistence.Configurations;

/// <summary>
/// Конфигурация хранения сообщений Outbox.
/// </summary>
public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    /// <summary>
    /// Настраивает таблицу сообщений Outbox.
    /// </summary>
    /// <param name="builder">Построитель конфигурации сущности.</param>
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.EventType)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(message => message.Payload)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(message => message.OccurredAtUtc)
            .IsRequired();

        builder.Property(message => message.Error)
            .HasColumnType("text");

        builder.Property(message => message.Attempts)
            .HasDefaultValue(0);

        builder.HasIndex(message => new
        {
            message.ProcessedAtUtc,
            message.FailedAtUtc,
            message.NextAttemptAtUtc,
            message.OccurredAtUtc
        });
    }
}
