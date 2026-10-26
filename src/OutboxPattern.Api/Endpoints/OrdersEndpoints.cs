using OutboxPattern.Api.Contracts;
using OutboxPattern.Application.Abstractions.CQRS;
using OutboxPattern.Application.UseCases.CreateOrder;

namespace OutboxPattern.Api.Endpoints;

/// <summary>
/// HTTP-эндпоинты для работы с заказами.
/// </summary>
public static class OrdersEndpoints
{
    /// <summary>
    /// Добавляет эндпоинты заказов в приложение.
    /// </summary>
    /// <param name="endpoints">Построитель эндпоинтов.</param>
    /// <returns>Тот же построитель эндпоинтов для цепочки вызовов.</returns>
    public static IEndpointRouteBuilder MapOrdersEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/orders")
            .WithTags("Orders");

        group.MapPost("/", CreateOrderAsync)
            .WithName("CreateOrder")
            .Produces<CreateOrderResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        return endpoints;
    }

    private static async Task<IResult> CreateOrderAsync(
        CreateOrderRequest request,
        ICommandHandler<CreateOrderCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var orderId = await handler.HandleAsync(
                new CreateOrderCommand
                {
                    CustomerEmail = request.CustomerEmail,
                    TotalPrice = request.TotalPrice
                },
                cancellationToken);

            return Results.Created(
                $"/orders/{orderId}",
                new CreateOrderResponse(orderId));
        }
        catch (ArgumentException exception)
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["order"] = [exception.Message]
                });
        }
    }
}
