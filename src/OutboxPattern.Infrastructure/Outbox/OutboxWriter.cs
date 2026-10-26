using OutboxPattern.Application.Abstractions.Outbox;
using OutboxPattern.Infrastructure.Persistence;

namespace OutboxPattern.Infrastructure.Outbox;

/// <summary>
/// Записывает интеграционные события в таблицу Outbox.
/// </summary>
/// <remarks>
/// Использует тот же <see cref="ApplicationDbContext"/>, что и UnitOfWork,
/// поэтому изменения попадают в общую транзакцию.
/// </remarks>
public sealed class OutboxWriter(
    ApplicationDbContext dbContext,
    IIntegrationEventSerializer serializer) : IOutboxWriter
{
    /// <inheritdoc />
    public async Task EnqueueAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent, nameof(integrationEvent));

        var serializedEvent = serializer.Serialize(integrationEvent);

        var outboxMessage = OutboxMessage.Create(
            serializedEvent.EventType,
            serializedEvent.Payload,
            integrationEvent.OccurredAtUtc);

        await dbContext.OutboxMessages.AddAsync(outboxMessage, cancellationToken);
    }
}
