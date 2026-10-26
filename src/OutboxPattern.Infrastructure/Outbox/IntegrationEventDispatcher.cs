using System.Collections.Concurrent;
using System.Linq.Expressions;

using Microsoft.Extensions.DependencyInjection;

using OutboxPattern.Application.Abstractions.Outbox;

namespace OutboxPattern.Infrastructure.Outbox;

/// <summary>
/// Передаёт интеграционное событие зарегистрированным обработчикам.
/// </summary>
/// <remarks>
/// Метаданные обработчиков и скомпилированные делегаты кэшируются по типу события,
/// поэтому рефлексия выполняется только при первой обработке типа.
/// </remarks>
public sealed class IntegrationEventDispatcher(
    IServiceProvider serviceProvider) : IIntegrationEventDispatcher
{
    private static readonly ConcurrentDictionary<Type, HandlerMetadata> MetadataCache = new();

    /// <inheritdoc />
    public async Task DispatchAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        var eventType = integrationEvent.GetType();
        var metadata = MetadataCache.GetOrAdd(
            eventType,
            static type => CreateMetadata(type));

        var handled = false;

        foreach (var handler in serviceProvider.GetServices(metadata.HandlerType))
        {
            if (handler is null)
            {
                continue;
            }

            handled = true;

            await metadata.Invoke(
                handler,
                integrationEvent,
                cancellationToken);
        }

        if (!handled)
        {
            throw new InvalidOperationException(
                $"Не зарегистрирован обработчик для события {eventType.FullName}.");
        }
    }

    private static HandlerMetadata CreateMetadata(Type eventType)
    {
        var handlerType = typeof(IIntegrationEventHandler<>)
            .MakeGenericType(eventType);

        var handleMethod = handlerType.GetMethod(
            nameof(IIntegrationEventHandler<>.HandleAsync))
            ?? throw new InvalidOperationException(
                $"Не удалось найти метод обработки события {eventType.FullName}.");

        var handlerParameter = Expression.Parameter(
            typeof(object),
            "handler");

        var integrationEventParameter = Expression.Parameter(
            typeof(IIntegrationEvent),
            "integrationEvent");

        var cancellationTokenParameter = Expression.Parameter(
            typeof(CancellationToken),
            "cancellationToken");

        var call = Expression.Call(
            Expression.Convert(handlerParameter, handlerType),
            handleMethod,
            Expression.Convert(integrationEventParameter, eventType),
            cancellationTokenParameter);

        var invoker = Expression.Lambda<HandlerInvoker>(
            call,
            handlerParameter,
            integrationEventParameter,
            cancellationTokenParameter).Compile();

        return new HandlerMetadata(handlerType, invoker);
    }

    private delegate Task HandlerInvoker(
        object handler,
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken);

    private sealed record HandlerMetadata(
        Type HandlerType,
        HandlerInvoker Invoke);
}
