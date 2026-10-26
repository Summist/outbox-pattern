namespace OutboxPattern.Application.Abstractions.Notifications;

/// <summary>
/// Отправляет email-сообщения.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Отправляет сообщение.
    /// </summary>
    /// <param name="message">Сообщение для отправки.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    Task SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default);
}
