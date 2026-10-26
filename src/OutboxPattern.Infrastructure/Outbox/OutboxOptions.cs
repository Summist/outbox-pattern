namespace OutboxPattern.Infrastructure.Outbox;

/// <summary>
/// Настройки фоновой обработки сообщений Outbox.
/// </summary>
public sealed class OutboxOptions
{
    /// <summary>
    /// Имя секции конфигурации.
    /// </summary>
    public const string SectionName = "Outbox";

    /// <summary>
    /// Интервал между циклами обработки.
    /// </summary>
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Максимальное количество сообщений, обрабатываемых за один цикл.
    /// </summary>
    public int BatchSize { get; set; } = 50;

    /// <summary>
    /// Максимальное количество попыток обработки одного сообщения.
    /// </summary>
    public int MaxAttempts { get; set; } = 5;

    /// <summary>
    /// Базовая задержка перед повторной попыткой.
    /// </summary>
    public TimeSpan RetryBaseDelay { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Максимальная задержка перед повторной попыткой.
    /// </summary>
    public TimeSpan RetryMaxDelay { get; set; } = TimeSpan.FromMinutes(1);
}
