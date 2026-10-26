namespace OutboxPattern.Infrastructure.Outbox;

/// <summary>
/// Сообщение Outbox, ожидающее обработки.
/// </summary>
/// <remarks>
/// Сохраняется в той же транзакции, что и изменения агрегатов,
/// и обрабатывается фоновым процессором.
/// </remarks>
public sealed class OutboxMessage
{
    private OutboxMessage()
    {
    }

    /// <summary>
    /// Уникальный идентификатор сообщения.
    /// </summary>
    /// <remarks>
    /// Создаётся как UUID v7, поэтому сообщения упорядочены по времени создания.
    /// </remarks>
    public Guid Id { get; private set; }

    /// <summary>
    /// Полное имя типа интеграционного события, необходимое для десериализации.
    /// </summary>
    public string EventType { get; private set; } = string.Empty;

    /// <summary>
    /// JSON-представление интеграционного события.
    /// </summary>
    public string Payload { get; private set; } = string.Empty;

    /// <summary>
    /// Дата и время возникновения события в UTC.
    /// </summary>
    public DateTimeOffset OccurredAtUtc { get; private set; }

    /// <summary>
    /// Дата и время успешной обработки в UTC; null, если сообщение ещё не обработано.
    /// </summary>
    public DateTimeOffset? ProcessedAtUtc { get; private set; }

    /// <summary>
    /// Дата и время, после которой сообщение можно повторить.
    /// null для новых сообщений и сообщений без назначенного повтора.
    /// </summary>
    public DateTimeOffset? NextAttemptAtUtc { get; private set; }

    /// <summary>
    /// Дата и время окончательного провала обработки.
    /// null, если сообщение ещё не провалено.
    /// </summary>
    public DateTimeOffset? FailedAtUtc { get; private set; }

    /// <summary>
    /// Текст последней ошибки обработки.
    /// </summary>
    public string? Error { get; private set; }

    /// <summary>
    /// Количество неудачных попыток обработки.
    /// </summary>
    public int Attempts { get; private set; }

    /// <summary>
    /// Создаёт сообщение Outbox для интеграционного события.
    /// </summary>
    /// <param name="eventType">Полное имя типа события.</param>
    /// <param name="payload">JSON-представление события.</param>
    /// <param name="occurredAtUtc">Дата и время возникновения события в UTC.</param>
    /// <returns>Новое сообщение Outbox.</returns>
    /// <exception cref="ArgumentException">
    /// Возникает, если тип события или payload пуст.
    /// </exception>
    public static OutboxMessage Create(
        string eventType,
        string payload,
        DateTimeOffset occurredAtUtc)
    {
        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new ArgumentException(
                "Тип интеграционного события обязателен.",
                nameof(eventType));
        }

        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new ArgumentException(
                "Payload интеграционного события обязателен.",
                nameof(payload));
        }

        return new OutboxMessage
        {
            Id = Guid.CreateVersion7(),
            EventType = eventType,
            Payload = payload,
            OccurredAtUtc = occurredAtUtc
        };
    }

    /// <summary>
    /// Отмечает сообщение как успешно обработанное.
    /// </summary>
    /// <param name="processedAtUtc">Дата и время обработки в UTC.</param>
    public void MarkAsProcessed(DateTimeOffset processedAtUtc)
    {
        ProcessedAtUtc = processedAtUtc;
        NextAttemptAtUtc = null;
        Error = null;
    }

    /// <summary>
    /// Фиксирует неудачную попытку обработки и планирует повтор.
    /// </summary>
    /// <param name="error">Описание ошибки.</param>
    /// <param name="nextAttemptAtUtc">
    /// Дата и время, после которой сообщение можно обработать повторно.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Возникает, если описание ошибки пусто.
    /// </exception>
    public void RegisterFailure(
        string error,
        DateTimeOffset nextAttemptAtUtc)
    {
        if (string.IsNullOrWhiteSpace(error))
            throw new ArgumentException("Описание ошибки обязательно.", nameof(error));

        Attempts++;
        Error = error;
        NextAttemptAtUtc = nextAttemptAtUtc;
    }

    /// <summary>
    /// Отмечает сообщение как окончательно проваленное.
    /// </summary>
    /// <param name="failedAtUtc">Дата и время окончательного провала в UTC.</param>
    public void MarkAsFailed(DateTimeOffset failedAtUtc)
    {
        FailedAtUtc = failedAtUtc;
        NextAttemptAtUtc = null;
    }
}
