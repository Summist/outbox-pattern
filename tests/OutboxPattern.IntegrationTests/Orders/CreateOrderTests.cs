using System.Net;
using System.Net.Http.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using OutboxPattern.Api.Contracts;
using OutboxPattern.Application.Abstractions.Outbox;
using OutboxPattern.Domain.Orders;
using OutboxPattern.Infrastructure.Outbox;
using OutboxPattern.Infrastructure.Persistence;
using OutboxPattern.IntegrationTests.Infrastructure;

namespace OutboxPattern.IntegrationTests.Orders;

[Collection(PostgresCollection.CollectionName)]
public sealed class CreateOrderTests : IAsyncLifetime
{
    private readonly PostgresFixture _postgres;
    private OutboxApiFactory _factory = null!;
    private HttpClient _client = null!;

    public CreateOrderTests(PostgresFixture postgres)
    {
        _postgres = postgres;
    }

    public async Task InitializeAsync()
    {
        _factory = new OutboxApiFactory(_postgres);
        await _factory.InitializeDatabaseAsync();
        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task CreateOrder_ValidRequest_PersistsOrderAndProcessesOutbox()
    {
        // Arrange
        var request = new CreateOrderRequest(
            "customer@example.com",
            1500m);

        // Act
        var response = await _client.PostAsJsonAsync("/orders", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content
            .ReadFromJsonAsync<CreateOrderResponse>();

        Assert.NotNull(created);

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            var order = await dbContext.Orders
                .SingleAsync(order => order.Id == created.OrderId);

            Assert.Equal("customer@example.com", order.CustomerEmail);
            Assert.Equal(1500m, order.TotalPrice);
            Assert.Equal(OrderStatus.Pending, order.Status);

            Assert.Equal(1, await dbContext.Orders.CountAsync());
            Assert.Equal(1, await dbContext.OutboxMessages.CountAsync());
        }

        await TestWait.UntilAsync(
            () => Task.FromResult(
                _factory.EmailSender.DeliveredMessages.Count == 1),
            TimeSpan.FromSeconds(10));

        var email = Assert.Single(_factory.EmailSender.DeliveredMessages);

        Assert.Equal("customer@example.com", email.To);
        Assert.Contains(created.OrderId.ToString(), email.Subject);
        Assert.Contains(created.OrderId.ToString(), email.Body);
        Assert.False(email.IsHtml);

        await TestWait.UntilAsync(
            async () =>
            {
                var message = await GetOutboxMessageAsync(_factory);
                return message.ProcessedAtUtc != null;
            },
            TimeSpan.FromSeconds(10));

        var processedMessage = await GetOutboxMessageAsync(_factory);

        Assert.NotNull(processedMessage.ProcessedAtUtc);
        Assert.Null(processedMessage.FailedAtUtc);
        Assert.Equal(0, processedMessage.Attempts);

        await AssertStatsAsync(
            expectedPending: 0,
            expectedProcessed: 1,
            expectedFailed: 0);
    }

    [Fact]
    public async Task CreateOrder_InvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateOrderRequest("invalid", 1500m);

        // Act
        var response = await _client.PostAsJsonAsync("/orders", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        await using var scope = _factory.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        Assert.False(await dbContext.Orders.AnyAsync());
        Assert.False(await dbContext.OutboxMessages.AnyAsync());
    }

    [Fact]
    public async Task GetSwaggerUi_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/swagger/index.html");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetOpenApiDocument_ContainsAllEndpoints()
    {
        // Act
        var document = await _client.GetStringAsync("/openapi/v1.json");

        // Assert
        Assert.Contains("/orders", document);
        Assert.Contains("/outbox/stats", document);
    }

    private async Task AssertStatsAsync(
        int expectedPending,
        int expectedProcessed,
        int expectedFailed)
    {
        var stats = await _client.GetFromJsonAsync<OutboxStats>("/outbox/stats");

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
