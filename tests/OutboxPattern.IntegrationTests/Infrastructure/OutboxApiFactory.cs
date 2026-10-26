using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using OutboxPattern.Application.Abstractions.Notifications;
using OutboxPattern.Infrastructure.Outbox;
using OutboxPattern.Infrastructure.Persistence;

namespace OutboxPattern.IntegrationTests.Infrastructure;

internal sealed class OutboxApiFactory(
    PostgresFixture postgres,
    TimeSpan? pollingInterval = null,
    int maxAttempts = 3,
    TimeSpan? retryBaseDelay = null,
    TimeSpan? retryMaxDelay = null) : WebApplicationFactory<Program>
{
    private readonly TimeSpan _pollingInterval =
        pollingInterval ?? TimeSpan.FromMilliseconds(100);

    private readonly TimeSpan _retryBaseDelay =
        retryBaseDelay ?? TimeSpan.FromMilliseconds(100);

    private readonly TimeSpan _retryMaxDelay =
        retryMaxDelay ?? TimeSpan.FromMilliseconds(500);

    public FakeEmailSender EmailSender { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting(
            "ConnectionStrings:Postgres",
            postgres.ConnectionString);

        builder.UseSetting("Smtp:Host", "localhost");
        builder.UseSetting("Smtp:Port", "1025");
        builder.UseSetting("Smtp:FromAddress", "noreply@outbox.test");
        builder.UseSetting("Smtp:FromName", "OutboxPattern Tests");
        builder.UseSetting("Smtp:EnableSsl", "false");
        builder.UseSetting("Smtp:Timeout", "00:00:05");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Postgres"] = postgres.ConnectionString,
                    ["Smtp:Host"] = "localhost",
                    ["Smtp:Port"] = "1025",
                    ["Smtp:FromAddress"] = "noreply@outbox.test",
                    ["Smtp:FromName"] = "OutboxPattern Tests",
                    ["Smtp:EnableSsl"] = "false",
                    ["Smtp:Timeout"] = "00:00:05"
                });
        });

        builder.ConfigureServices(services =>
        {
            services.PostConfigure<OutboxOptions>(options =>
            {
                options.PollingInterval = _pollingInterval;
                options.BatchSize = 10;
                options.MaxAttempts = maxAttempts;
                options.RetryBaseDelay = _retryBaseDelay;
                options.RetryMaxDelay = _retryMaxDelay;
            });

            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(EmailSender);
        });
    }

    public async Task MigrateDatabaseAsync()
    {
        await using var scope = Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.MigrateAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        await using var scope = Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        await dbContext.OutboxMessages.ExecuteDeleteAsync();
        await dbContext.Orders.ExecuteDeleteAsync();
    }

    public async Task InitializeDatabaseAsync()
    {
        await MigrateDatabaseAsync();
        await ResetDatabaseAsync();
    }
}
