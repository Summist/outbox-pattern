namespace OutboxPattern.Application.Abstractions.Notifications;

/// <summary>
/// Содержимое email-сообщения.
/// </summary>
public sealed record EmailMessage
{
    /// <summary>
    /// Адрес получателя.
    /// </summary>
    public required string To { get; init; }

    /// <summary>
    /// Тема сообщения.
    /// </summary>
    public required string Subject { get; init; }

    /// <summary>
    /// Текст сообщения.
    /// </summary>
    public required string Body { get; init; }

    /// <summary>
    /// Признак того, что тело сообщения содержит HTML.
    /// По умолчанию письмо отправляется как обычный текст.
    /// </summary>
    public bool IsHtml { get; init; }
}
