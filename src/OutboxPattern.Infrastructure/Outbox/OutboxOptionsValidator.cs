using Microsoft.Extensions.Options;

namespace OutboxPattern.Infrastructure.Outbox;

/// <summary>
/// Проверяет корректность настроек обработки Outbox.
/// </summary>
public sealed class OutboxOptionsValidator : IValidateOptions<OutboxOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, OutboxOptions options)
    {
        var failures = new List<string>();

        if (options.PollingInterval <= TimeSpan.Zero)
        {
            failures.Add("Outbox:PollingInterval должен быть больше нуля.");
        }

        if (options.BatchSize <= 0)
        {
            failures.Add("Outbox:BatchSize должен быть больше нуля.");
        }

        if (options.MaxAttempts <= 0)
        {
            failures.Add("Outbox:MaxAttempts должен быть больше нуля.");
        }

        if (options.RetryBaseDelay <= TimeSpan.Zero)
        {
            failures.Add("Outbox:RetryBaseDelay должен быть больше нуля.");
        }

        if (options.RetryMaxDelay < options.RetryBaseDelay)
        {
            failures.Add(
                "Outbox:RetryMaxDelay не должен быть меньше Outbox:RetryBaseDelay.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
