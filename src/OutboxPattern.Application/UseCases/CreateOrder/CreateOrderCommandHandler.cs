using OutboxPattern.Application.Abstractions.CQRS;
using OutboxPattern.Application.Abstractions.Outbox;
using OutboxPattern.Application.Abstractions.Persistence;
using OutboxPattern.Application.Abstractions.Persistence.Repositories;
using OutboxPattern.Application.Events.Orders;

using OutboxPattern.Domain.Orders;

namespace OutboxPattern.Application.UseCases.CreateOrder;

internal sealed class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    IOutboxWriter outboxWriter,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateOrderCommand, Guid>
{
    public async Task<Guid> HandleAsync(
        CreateOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        var order = Order.Create(
            command.CustomerEmail,
            command.TotalPrice);

        await orderRepository.AddAsync(order, cancellationToken);

        await outboxWriter.EnqueueAsync(
            OrderCreatedIntegrationEvent.Create(order),
            cancellationToken);

        _ = await unitOfWork.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}
