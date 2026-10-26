namespace OutboxPattern.Application.Abstractions.Outbox;

/// <summary>
/// Определяет контракт интеграционного события.
/// </summary>
/// <remarks>
/// Событие сериализуется в Outbox и передаётся внешним потребителям,
/// поэтому изменения контракта должны сохранять обратную совместимость.
/// </remarks>
public interface IIntegrationEvent
{
    /// <summary>
    /// Дата и время возникновения события в UTC.
    /// </summary>
    DateTimeOffset OccurredAtUtc { get; }
}
