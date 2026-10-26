using System.Net;
using System.Net.Mail;
using System.Text;

using Microsoft.Extensions.Options;

using OutboxPattern.Application.Abstractions.Notifications;

namespace OutboxPattern.Infrastructure.Notifications;

/// <summary>
/// Отправляет email-сообщения через SMTP.
/// </summary>
/// <remarks>
/// Для каждого сообщения создаётся отдельный <see cref="SmtpClient"/>,
/// поэтому реализация безопасна при параллельных вызовах.
/// </remarks>
public sealed class EmailSender(
    IOptions<SmtpOptions> options) : IEmailSender
{
    private readonly SmtpOptions _options = options.Value;

    /// <inheritdoc />
    public async Task SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (string.IsNullOrWhiteSpace(message.To))
        {
            throw new ArgumentException(
                "Адрес получателя обязателен.",
                nameof(message));
        }

        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl,
            Timeout = (int)_options.Timeout.TotalMilliseconds,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        if (!string.IsNullOrWhiteSpace(_options.UserName))
        {
            client.Credentials = new NetworkCredential(
                _options.UserName,
                _options.Password);
        }

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(
                _options.FromAddress,
                _options.FromName,
                Encoding.UTF8),
            To = { message.To },
            Subject = message.Subject,
            SubjectEncoding = Encoding.UTF8,
            Body = message.Body,
            BodyEncoding = Encoding.UTF8,
            IsBodyHtml = message.IsHtml
        };

        await client
            .SendMailAsync(mailMessage, cancellationToken)
            .ConfigureAwait(false);
    }
}
