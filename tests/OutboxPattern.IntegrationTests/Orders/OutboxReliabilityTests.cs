using System.Net;
using System.Net.Http.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using OutboxPattern.Api.Contracts;
using OutboxPattern.Application.Abstractions.Outbox;
using OutboxPattern.Infrastructure.Outbox;
using OutboxPattern.Infrastructure.Persistence;
using OutboxPattern.IntegrationTests.Infrastructure;

namespace OutboxPattern.IntegrationTests.Orders;

[Collection(PostgresCollection.CollectionName)]
public sealed class OutboxReliabilityTests
{
    private readonly PostgresFixture _postgres;

    public OutboxReliabilityTests(PostgresFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task CreateOrder_TransientEmailFailure_RetriesAndDeliversOnce()
    {
        // Arrange
        await using var factory = new OutboxApiFactory(
            _postgres,
            maxAttempts: 3,
            retryBaseDelay: TimeSpan.FromMilliseconds(100),
            retryMaxDelay: TimeSpan.FromMilliseconds(500));

        await factory.InitializeDatabaseAsync();

        using var client = factory.CreateClient();
        factory.EmailSender.FailuresBeforeSuccess = 1;

        // Act
        var response = await CreateOrderAsync(client);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        await TestWait.UntilAsync(
            async () => (await GetOutboxMessageAsync(factory)).ProcessedAtUtc != null,
            TimeSpan.FromSeconds(10));

        var message = await GetOutboxMessageAsync(factory);

        Assert.NotNull(message.ProcessedAtUtc);
        Assert.Null(message.FailedAtUtc);
        Assert.Equal(1, message.Attempts);
        Assert.Equal(2, factory.EmailSender.Attempts);
        Assert.Single(factory.EmailSender.DeliveredMessages);

        await AssertStatsAsync(
            client,
            expectedPending: 0,
            expectedProcessed: 1,
            expectedFailed: 0);
    }

    [Fact]
    public async Task CreateOrder_PermanentEmailFailure_MarksMessageFailedWithoutLoss()
    {
        // Arrange
        await using var factory = new OutboxApiFactory(
            _postgres,
            maxAttempts: 2,
            retryBaseDelay: TimeSpan.FromMilliseconds(100),
            retryMaxDelay: TimeSpan.FromMilliseconds(200));

        await factory.InitializeDatabaseAsync();

        using var client = factory.CreateClient();
        factory.EmailSender.FailuresBeforeSuccess = int.MaxValue;

        // Act
        var response = await CreateOrderAsync(client);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        await TestWait.UntilAsync(
            async () => (await GetOutboxMessageAsync(factory)).FailedAtUtc != null,
            TimeSpan.FromSeconds(10));

        var message = await GetOutboxMessageAsync(factory);

        Assert.Null(message.ProcessedAtUtc);
        Assert.NotNull(message.FailedAtUtc);
        Assert.Equal(2, message.Attempts);
        Assert.Equal(2, factory.EmailSender.Attempts);
        Assert.Empty(factory.EmailSender.DeliveredMessages);

        await AssertStatsAsync(
            client,
            expectedPending: 0,
            expectedProcessed: 0,
            expectedFailed: 1);
    }

    [Fact]
    public async Task OutboxMessage_SurvivesApiRestartAndProcessedOnce()
    {
        // Arrange: процессор не успевает обработать сообщение.
        await using (var firstFactory = new OutboxApiFactory(
            _postgres,
            pollingInterval: TimeSpan.FromMinutes(1)))
        {
            await firstFactory.InitializeDatabaseAsync();

            using var firstClient = firstFactory.CreateClient();

            var response = await CreateOrderAsync(firstClient);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var pendingMessage = await GetOutboxMessageAsync(firstFactory);

            Assert.Null(pendingMessage.ProcessedAtUtc);
            Assert.Null(pendingMessage.FailedAtUtc);
            Assert.Empty(firstFactory.EmailSender.DeliveredMessages);
        }

        // Act: новый экземпляр приложения обрабатывает сохранённое сообщение.
        await using var secondFactory = new OutboxApiFactory(
            _postgres,
            pollingInterval: TimeSpan.FromMilliseconds(100));

        await secondFactory.MigrateDatabaseAsync();

        using var secondClient = secondFactory.CreateClient();

        await TestWait.UntilAsync(
            async () => (await GetOutboxMessageAsync(secondFactory)).ProcessedAtUtc != null,
            TimeSpan.FromSeconds(10));

        // Assert
        var processedMessage = await GetOutboxMessageAsync(secondFactory);

        Assert.NotNull(processedMessage.ProcessedAtUtc);
        Assert.Null(processedMessage.FailedAtUtc);
        Assert.Equal(0, processedMessage.Attempts);
        Assert.Single(secondFactory.EmailSender.DeliveredMessages);

        await AssertStatsAsync(
            secondClient,
            expectedPending: 0,
            expectedProcessed: 1,
            expectedFailed: 0);
    }

    private static Task<HttpResponseMessage> CreateOrderAsync(HttpClient client)
    {
        return client.PostAsJsonAsync(
            "/orders",
            new CreateOrderRequest("customer@example.com", 1500m));
    }

    private static async Task AssertStatsAsync(
        HttpClient client,
        int expectedPending,
        int expectedProcessed,
        int expectedFailed)
    {
        var stats = await client.GetFromJsonAsync<OutboxStats>("/outbox/stats");

        Assert.NotNull(stats);
        Assert.Equal(expectedPending, stats.Pending);
        Assert.Equal(expectedProcessed, stats.Processed);
        Assert.Equal(expectedFailed, stats.Failed);
    }

    private static async Task<OutboxMessage> GetOutboxMessageAsync(
        OutboxApiFactory factory)
    {
        await using var scope = factory.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        return await dbContext.OutboxMessages.SingleAsync();
    }
}
