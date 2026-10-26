using System.ComponentModel.DataAnnotations;

namespace OutboxPattern.Infrastructure.Notifications;

/// <summary>
/// Настройки отправки email через SMTP.
/// </summary>
public sealed class SmtpOptions
{
    /// <summary>
    /// Имя секции конфигурации.
    /// </summary>
    public const string SectionName = "Smtp";

    /// <summary>
    /// Адрес SMTP-сервера.
    /// </summary>
    [Required]
    public string Host { get; init; } = string.Empty;

    /// <summary>
    /// Порт SMTP-сервера.
    /// </summary>
    [Range(1, 65535)]
    public int Port { get; init; } = 587;

    /// <summary>
    /// Имя пользователя для аутентификации.
    /// null, если сервер не требует аутентификации.
    /// </summary>
    public string? UserName { get; init; }

    /// <summary>
    /// Пароль для аутентификации.
    /// null, если сервер не требует аутентификации.
    /// </summary>
    public string? Password { get; init; }

    /// <summary>
    /// Адрес отправителя.
    /// </summary>
    [Required]
    [EmailAddress]
    public string FromAddress { get; init; } = string.Empty;

    /// <summary>
    /// Отображаемое имя отправителя.
    /// </summary>
    public string? FromName { get; init; }

    /// <summary>
    /// Использовать ли защищённое соединение.
    /// </summary>
    public bool EnableSsl { get; init; } = true;

    /// <summary>
    /// Таймаут отправки сообщения.
    /// </summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
}
