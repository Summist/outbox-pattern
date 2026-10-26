using System.Globalization;

using OutboxPattern.Application.Abstractions.Notifications;
using OutboxPattern.Application.Abstractions.Outbox;
using OutboxPattern.Application.Events.Orders;

namespace OutboxPattern.Application.EventHandlers.Orders;

/// <summary>
/// Отправляет покупателю письмо после создания заказа.
/// </summary>
internal sealed class OrderCreatedIntegrationEventHandler(
    IEmailSender emailSender)
    : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
{
    /// <inheritdoc />
    public Task HandleAsync(
        OrderCreatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var totalPrice = integrationEvent.TotalPrice.ToString(
            "0.00",
            CultureInfo.InvariantCulture);

        var occurredAtUtc = integrationEvent.OccurredAtUtc.ToString(
            "dd.MM.yyyy HH:mm",
            CultureInfo.InvariantCulture);

        var message = new EmailMessage
        {
            To = integrationEvent.CustomerEmail,
            Subject = $"Заказ №{integrationEvent.OrderId} создан",
            Body = $"""
                Здравствуйте!

                Ваш заказ №{integrationEvent.OrderId} успешно создан.
                Сумма заказа: {totalPrice}.
                Дата создания: {occurredAtUtc} UTC.

                Спасибо за покупку!
                """,
            IsHtml = false
        };

        return emailSender.SendAsync(message, cancellationToken);
    }
}
