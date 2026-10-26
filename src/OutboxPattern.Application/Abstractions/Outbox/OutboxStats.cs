namespace OutboxPattern.Application.Abstractions.Outbox;

/// <summary>
/// Статистика сообщений Outbox.
/// </summary>
/// <param name="Pending">Количество сообщений, ожидающих обработки.</param>
/// <param name="Processed">Количество успешно обработанных сообщений.</param>
/// <param name="Failed">Количество сообщений с окончательным провалом.</param>
/// <param name="OldestPendingAtUtc">
/// Время возникновения самого старого необработанного сообщения.
/// </param>
public sealed record OutboxStats(
    int Pending,
    int Processed,
    int Failed,
    DateTimeOffset? OldestPendingAtUtc);
