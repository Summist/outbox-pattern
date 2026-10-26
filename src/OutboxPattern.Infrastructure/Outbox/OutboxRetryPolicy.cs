namespace OutboxPattern.Infrastructure.Outbox;

/// <summary>
/// Рассчитывает задержку перед повторной обработкой сообщения Outbox.
/// </summary>
public static class OutboxRetryPolicy
{
    /// <summary>
    /// Возвращает экспоненциально растущую задержку с ограничением сверху.
    /// </summary>
    /// <param name="attempt">Номер попытки, начиная с 1.</param>
    /// <param name="options">Настройки обработки Outbox.</param>
    /// <returns>Задержка перед следующей попыткой.</returns>
    /// <exception cref="ArgumentNullException">
    /// Возникает, если <paramref name="options"/> равен <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Возникает, если номер попытки меньше 1 или задержки в настройках не положительны.
    /// </exception>
    public static TimeSpan GetDelay(int attempt, OutboxOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (attempt < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(attempt),
                attempt,
                "Номер попытки должен быть больше нуля.");
        }

        if (options.RetryBaseDelay <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "Базовая задержка должна быть больше нуля.");
        }

        if (options.RetryMaxDelay <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "Максимальная задержка должна быть больше нуля.");
        }

        var delayTicks = options.RetryBaseDelay.Ticks
            * Math.Pow(2, attempt - 1);

        var cappedTicks = Math.Min(
            delayTicks,
            options.RetryMaxDelay.Ticks);

        return TimeSpan.FromTicks((long)cappedTicks);
    }
}
