using Testcontainers.PostgreSql;

namespace OutboxPattern.IntegrationTests.Infrastructure;

/// <summary>
/// Запускает PostgreSQL в контейнере для интеграционных тестов.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("outbox_pattern")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    /// <summary>
    /// Строка подключения к тестовой базе данных.
    /// </summary>
    public string ConnectionString => _container.GetConnectionString();

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        Environment.SetEnvironmentVariable(
            "ConnectionStrings__Postgres",
            ConnectionString);

        Environment.SetEnvironmentVariable("Smtp__Host", "localhost");
        Environment.SetEnvironmentVariable("Smtp__Port", "1025");
        Environment.SetEnvironmentVariable("Smtp__FromAddress", "noreply@outbox.test");
        Environment.SetEnvironmentVariable("Smtp__FromName", "OutboxPattern Tests");
        Environment.SetEnvironmentVariable("Smtp__EnableSsl", "false");
        Environment.SetEnvironmentVariable("Smtp__Timeout", "00:00:05");
    }

    /// <inheritdoc />
    public async Task DisposeAsync()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__Postgres", null);
        Environment.SetEnvironmentVariable("Smtp__Host", null);
        Environment.SetEnvironmentVariable("Smtp__Port", null);
        Environment.SetEnvironmentVariable("Smtp__FromAddress", null);
        Environment.SetEnvironmentVariable("Smtp__FromName", null);
        Environment.SetEnvironmentVariable("Smtp__EnableSsl", null);
        Environment.SetEnvironmentVariable("Smtp__Timeout", null);

        await _container.DisposeAsync();
    }
}
