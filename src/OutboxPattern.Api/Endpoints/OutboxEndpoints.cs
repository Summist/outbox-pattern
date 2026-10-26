using OutboxPattern.Application.Abstractions.Outbox;

namespace OutboxPattern.Api.Endpoints;

/// <summary>
/// HTTP-эндпоинты для мониторинга Outbox.
/// </summary>
public static class OutboxEndpoints
{
    /// <summary>
    /// Добавляет эндпоинты мониторинга Outbox.
    /// </summary>
    /// <param name="endpoints">Построитель эндпоинтов.</param>
    /// <returns>Тот же построитель эндпоинтов для цепочки вызовов.</returns>
    public static IEndpointRouteBuilder MapOutboxEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/outbox/stats", GetStatsAsync)
            .WithName("GetOutboxStats")
            .WithTags("Outbox")
            .Produces<OutboxStats>();

        return endpoints;
    }

    private static async Task<IResult> GetStatsAsync(
        IOutboxStatsReader reader,
        CancellationToken cancellationToken)
    {
        var stats = await reader.GetAsync(cancellationToken);
        return Results.Ok(stats);
    }
}
