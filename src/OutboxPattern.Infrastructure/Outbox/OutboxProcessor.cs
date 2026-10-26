using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using OutboxPattern.Application.Abstractions.Outbox;
using OutboxPattern.Infrastructure.Persistence;

namespace OutboxPattern.Infrastructure.Outbox;

/// <summary>
/// Фоновая обработка сообщений Outbox.
/// </summary>
/// <remarks>
/// Периодически читает необработанные сообщения и передаёт их
/// зарегистрированным обработчикам интеграционных событий.
/// Неудачные попытки повторяются с экспоненциально растущей задержкой.
/// </remarks>
public sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    IIntegrationEventSerializer serializer,
    IOptions<OutboxOptions> options,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    private readonly OutboxOptions _options = options.Value;

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_options.PollingInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Не удалось обработать пачку Outbox-сообщений");
            }
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IIntegrationEventDispatcher>();

        var now = DateTimeOffset.UtcNow;

        var messages = await dbContext.OutboxMessages
            .Where(message =>
                message.ProcessedAtUtc == null &&
                message.FailedAtUtc == null &&
                (message.NextAttemptAtUtc == null ||
                 message.NextAttemptAtUtc <= now))
            .OrderBy(message => message.OccurredAtUtc)
            .Take(_options.BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                var integrationEvent = serializer.Deserialize(
                    message.EventType,
                    message.Payload);

                await dispatcher.DispatchAsync(integrationEvent, cancellationToken);

                message.MarkAsProcessed(DateTimeOffset.UtcNow);

                logger.LogInformation(
                    "Outbox-сообщение {MessageId} обработано.",
                    message.Id);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                var failedAtUtc = DateTimeOffset.UtcNow;
                var nextAttemptAtUtc = failedAtUtc.Add(
                    OutboxRetryPolicy.GetDelay(
                        message.Attempts + 1,
                        _options));

                message.RegisterFailure(exception.Message, nextAttemptAtUtc);

                if (message.Attempts >= _options.MaxAttempts)
                {
                    message.MarkAsFailed(failedAtUtc);

                    logger.LogError(
                        exception,
                        "Исчерпаны попытки обработки Outbox-сообщения {MessageId}.",
                        message.Id);
                }
                else
                {
                    logger.LogWarning(
                        exception,
                        "Ошибка обработки Outbox-сообщения {MessageId}. Повтор после {NextAttemptAtUtc}.",
                        message.Id,
                        nextAttemptAtUtc);
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
