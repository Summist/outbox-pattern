namespace OutboxPattern.IntegrationTests.Infrastructure;

internal static class TestWait
{
    public static async Task UntilAsync(
        Func<Task<bool>> condition,
        TimeSpan timeout)
    {
        var deadline = DateTimeOffset.UtcNow + timeout;

        while (!await condition())
        {
            if (DateTimeOffset.UtcNow >= deadline)
            {
                throw new TimeoutException(
                    "Условие не выполнено за отведённое время.");
            }

            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }
    }
}
