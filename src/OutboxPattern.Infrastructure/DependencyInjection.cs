using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using OutboxPattern.Application.Abstractions.Notifications;
using OutboxPattern.Application.Abstractions.Outbox;
using OutboxPattern.Application.Abstractions.Persistence;
using OutboxPattern.Application.Abstractions.Persistence.Repositories;
using OutboxPattern.Infrastructure.Notifications;
using OutboxPattern.Infrastructure.Outbox;
using OutboxPattern.Infrastructure.Persistence;
using OutboxPattern.Infrastructure.Persistence.Repositories;

namespace OutboxPattern.Infrastructure;

/// <summary>
/// Регистрация зависимостей инфраструктурного слоя.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Добавляет EF Core, PostgreSQL, Outbox, SMTP и фоновый процессор.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="configuration">Конфигурация приложения.</param>
    /// <returns>Та же коллекция сервисов для цепочки вызовов.</returns>
    /// <exception cref="ArgumentNullException">
    /// Возникает, если <paramref name="services"/> или <paramref name="configuration"/>
    /// равен <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Возникает, если не задана строка подключения <c>Postgres</c>.
    /// </exception>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException(
                "Строка подключения 'Postgres' не настроена.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork>(serviceProvider =>
            serviceProvider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOutboxWriter, OutboxWriter>();
        services.AddScoped<IOutboxStatsReader, OutboxStatsReader>();
        services.AddSingleton<IIntegrationEventSerializer, JsonIntegrationEventSerializer>();
        services.AddScoped<IIntegrationEventDispatcher, IntegrationEventDispatcher>();
        services.AddScoped<IEmailSender, EmailSender>();

        services.AddOptions<OutboxOptions>()
            .Bind(configuration.GetSection(OutboxOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<OutboxOptions>, OutboxOptionsValidator>();

        services.AddOptions<SmtpOptions>()
            .Bind(configuration.GetSection(SmtpOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<SmtpOptions>, SmtpOptionsValidator>();
        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}
