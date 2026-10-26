using Microsoft.Extensions.Options;

namespace OutboxPattern.Infrastructure.Notifications;

/// <summary>
/// Проверяет корректность настроек SMTP.
/// </summary>
public sealed class SmtpOptionsValidator : IValidateOptions<SmtpOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, SmtpOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Host))
        {
            failures.Add("Smtp:Host обязателен.");
        }

        if (options.Port is < 1 or > 65535)
        {
            failures.Add("Smtp:Port должен быть в диапазоне 1..65535.");
        }

        if (string.IsNullOrWhiteSpace(options.FromAddress))
        {
            failures.Add("Smtp:FromAddress обязателен.");
        }

        if (string.IsNullOrWhiteSpace(options.UserName) !=
            string.IsNullOrWhiteSpace(options.Password))
        {
            failures.Add("Smtp:UserName и Smtp:Password должны быть заданы вместе.");
        }

        if (options.Timeout <= TimeSpan.Zero)
        {
            failures.Add("Smtp:Timeout должен быть больше нуля.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
