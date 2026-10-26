using OutboxPattern.Infrastructure.Outbox;

namespace OutboxPattern.UnitTests.Infrastructure.Outbox;

public sealed class OutboxRetryPolicyTests
{
    private static readonly OutboxOptions Options = new()
    {
        RetryBaseDelay = TimeSpan.FromSeconds(2),
        RetryMaxDelay = TimeSpan.FromSeconds(10)
    };

    [Theory]
    [InlineData(1, 2)]
    [InlineData(2, 4)]
    [InlineData(3, 8)]
    [InlineData(4, 10)]
    public void GetDelay_ReturnsExponentialDelayWithCap(
        int attempt,
        int expectedSeconds)
    {
        // Act
        var delay = OutboxRetryPolicy.GetDelay(attempt, Options);

        // Assert
        Assert.Equal(TimeSpan.FromSeconds(expectedSeconds), delay);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GetDelay_InvalidAttempt_ThrowsArgumentOutOfRangeException(
        int attempt)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => OutboxRetryPolicy.GetDelay(attempt, Options));
    }

    [Fact]
    public void GetDelay_NullOptions_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => OutboxRetryPolicy.GetDelay(1, null!));
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(2, 0)]
    public void GetDelay_NonPositiveDelays_ThrowsArgumentOutOfRangeException(
        int baseSeconds,
        int maxSeconds)
    {
        // Arrange
        var options = new OutboxOptions
        {
            RetryBaseDelay = TimeSpan.FromSeconds(baseSeconds),
            RetryMaxDelay = TimeSpan.FromSeconds(maxSeconds)
        };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => OutboxRetryPolicy.GetDelay(1, options));
    }
}
