using OutboxPattern.Infrastructure.Outbox;

namespace OutboxPattern.UnitTests.Infrastructure.Outbox;

public sealed class OutboxMessageTests
{
    private static readonly DateTimeOffset OccurredAtUtc =
        new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void RegisterFailure_IncrementsAttemptsAndSchedulesRetry()
    {
        // Arrange
        var message = CreateMessage();
        var nextAttemptAtUtc = OccurredAtUtc.AddSeconds(5);

        // Act
        message.RegisterFailure("Временная ошибка", nextAttemptAtUtc);

        // Assert
        Assert.Equal(1, message.Attempts);
        Assert.Equal("Временная ошибка", message.Error);
        Assert.Equal(nextAttemptAtUtc, message.NextAttemptAtUtc);
        Assert.Null(message.FailedAtUtc);
    }

    [Fact]
    public void MarkAsProcessed_ClearsErrorAndNextAttempt()
    {
        // Arrange
        var message = CreateMessage();
        message.RegisterFailure(
            "Временная ошибка",
            OccurredAtUtc.AddSeconds(5));

        var processedAtUtc = OccurredAtUtc.AddSeconds(10);

        // Act
        message.MarkAsProcessed(processedAtUtc);

        // Assert
        Assert.Equal(processedAtUtc, message.ProcessedAtUtc);
        Assert.Null(message.Error);
        Assert.Null(message.NextAttemptAtUtc);
    }

    [Fact]
    public void MarkAsFailed_SetsFailedAtUtcAndClearsNextAttempt()
    {
        // Arrange
        var message = CreateMessage();
        message.RegisterFailure(
            "Постоянная ошибка",
            OccurredAtUtc.AddSeconds(5));

        var failedAtUtc = OccurredAtUtc.AddSeconds(10);

        // Act
        message.MarkAsFailed(failedAtUtc);

        // Assert
        Assert.Equal(failedAtUtc, message.FailedAtUtc);
        Assert.Null(message.NextAttemptAtUtc);
        Assert.Equal("Постоянная ошибка", message.Error);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void RegisterFailure_EmptyError_ThrowsArgumentException(string error)
    {
        // Arrange
        var message = CreateMessage();

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => message.RegisterFailure(
                error,
                OccurredAtUtc.AddSeconds(5)));
    }

    private static OutboxMessage CreateMessage()
    {
        return OutboxMessage.Create(
            "OutboxPattern.Application.Events.OrderCreatedIntegrationEvent",
            "{}",
            OccurredAtUtc);
    }
}
