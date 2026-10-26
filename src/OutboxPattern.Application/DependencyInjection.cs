using Microsoft.Extensions.DependencyInjection;

using OutboxPattern.Application.Abstractions.CQRS;
using OutboxPattern.Application.Abstractions.Outbox;
using OutboxPattern.Application.EventHandlers.Orders;
using OutboxPattern.Application.Events.Orders;
using OutboxPattern.Application.UseCases.CreateOrder;

namespace OutboxPattern.Application;

/// <summary>
/// Регистрация зависимостей прикладного слоя.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Добавляет обработчики команд и интеграционных событий.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <returns>Та же коллекция сервисов для цепочки вызовов.</returns>
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<
            ICommandHandler<CreateOrderCommand, Guid>,
            CreateOrderCommandHandler>();

        services.AddScoped<
            IIntegrationEventHandler<OrderCreatedIntegrationEvent>,
            OrderCreatedIntegrationEventHandler>();

        return services;
    }
}
