using Microsoft.EntityFrameworkCore;

using OutboxPattern.Application.Abstractions.Outbox;
using OutboxPattern.Infrastructure.Persistence;

namespace OutboxPattern.Infrastructure.Outbox;

/// <summary>
/// Возвращает статистику Outbox из PostgreSQL.
/// </summary>
internal sealed class OutboxStatsReader(
    ApplicationDbContext dbContext) : IOutboxStatsReader
{
    /// <inheritdoc />
    public async Task<OutboxStats> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var pending = await dbContext.OutboxMessages
            .CountAsync(
                message =>
                    message.ProcessedAtUtc == null &&
                    message.FailedAtUtc == null,
                cancellationToken);

        var processed = await dbContext.OutboxMessages
            .CountAsync(
                message => message.ProcessedAtUtc != null,
                cancellationToken);

        var failed = await dbContext.OutboxMessages
            .CountAsync(
                message => message.FailedAtUtc != null,
                cancellationToken);

        var oldestPendingAtUtc = await dbContext.OutboxMessages
            .Where(message =>
                message.ProcessedAtUtc == null &&
                message.FailedAtUtc == null)
            .MinAsync(
                message => (DateTimeOffset?)message.OccurredAtUtc,
                cancellationToken);

        return new OutboxStats(
            pending,
            processed,
            failed,
            oldestPendingAtUtc);
    }
}
