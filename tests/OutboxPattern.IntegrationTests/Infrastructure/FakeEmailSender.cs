using System.Collections.Concurrent;

using OutboxPattern.Application.Abstractions.Notifications;

namespace OutboxPattern.IntegrationTests.Infrastructure;

internal sealed class FakeEmailSender : IEmailSender
{
    private readonly ConcurrentQueue<EmailMessage> _deliveredMessages = new();
    private int _attempts;

    /// <summary>
    /// Количество первых попыток, которые должны завершиться ошибкой.
    /// </summary>
    public int FailuresBeforeSuccess { get; set; }

    /// <summary>
    /// Общее количество попыток отправки.
    /// </summary>
    public int Attempts => Volatile.Read(ref _attempts);

    /// <summary>
    /// Успешно доставленные сообщения.
    /// </summary>
    public IReadOnlyCollection<EmailMessage> DeliveredMessages =>
        _deliveredMessages.ToArray();

    public Task SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default)
    {
        var attempt = Interlocked.Increment(ref _attempts);

        if (attempt <= FailuresBeforeSuccess)
        {
            throw new InvalidOperationException(
                "Имитация временной недоступности SMTP.");
        }

        _deliveredMessages.Enqueue(message);

        return Task.CompletedTask;
    }
}
