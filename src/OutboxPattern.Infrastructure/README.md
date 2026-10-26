# OutboxPattern.Infrastructure

## 1. Назначение

Инфраструктурный слой реализует хранение данных, Transactional Outbox, SMTP-отправку и фоновую обработку событий.

## 2. Структура

```text
OutboxPattern.Infrastructure/
├── Notifications/
│   ├── EmailSender.cs
│   ├── SmtpOptions.cs
│   └── SmtpOptionsValidator.cs
├── Outbox/
│   ├── IntegrationEventDispatcher.cs
│   ├── JsonIntegrationEventSerializer.cs
│   ├── OutboxMessage.cs
│   ├── OutboxOptions.cs
│   ├── OutboxProcessor.cs
│   ├── OutboxRetryPolicy.cs
│   ├── OutboxStatsReader.cs
│   └── OutboxWriter.cs
├── Persistence/
│   ├── ApplicationDbContext.cs
│   ├── Configurations/
│   ├── Migrations/
│   └── Repositories/
└── DependencyInjection.cs
```

## 3. PostgreSQL и EF Core

`ApplicationDbContext` реализует `IUnitOfWork` через унаследованный `SaveChangesAsync`.

Конфигурации сущностей находятся в `Persistence/Configurations` и подключаются через `ApplyConfigurationsFromAssembly`.

Строка подключения:

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=outbox_pattern;Username=postgres;Password=postgres"
  }
}
```

Для локального запуска используется `docker compose up -d`.

pgAdmin доступен на `http://localhost:5050` (логин `admin@outboxpattern.io`, пароль `admin`). Сервер PostgreSQL добавляется автоматически из `servers.json`.

## 4. Таблица OutboxMessages

Основные поля:

| Поле | Назначение |
|---|---|
| `Id` | UUID v7, первичный ключ |
| `EventType` | Assembly-qualified имя типа события |
| `Payload` | JSON события, колонка `jsonb` |
| `OccurredAtUtc` | Время возникновения события |
| `ProcessedAtUtc` | Время успешной обработки |
| `NextAttemptAtUtc` | Время следующей попытки |
| `FailedAtUtc` | Время окончательного провала |
| `Error` | Текст последней ошибки |
| `Attempts` | Количество неудачных попыток |

Индекс: `ProcessedAtUtc`, `FailedAtUtc`, `NextAttemptAtUtc`, `OccurredAtUtc`.

## 5. OutboxProcessor

`OutboxProcessor` — `BackgroundService`, который:

1. раз в `Outbox:PollingInterval` читает пачку сообщений;
2. выбирает только сообщения без `ProcessedAtUtc` и `FailedAtUtc`, у которых наступил `NextAttemptAtUtc`;
3. десериализует событие;
4. вызывает `IIntegrationEventDispatcher`;
5. при успехе ставит `ProcessedAtUtc`;
6. при ошибке увеличивает `Attempts`, сохраняет `Error` и планирует повтор;
7. после `MaxAttempts` ставит `FailedAtUtc`.

Обработка рассчитана на один экземпляр приложения. Для нескольких экземпляров понадобится блокировка `FOR UPDATE SKIP LOCKED`.

`OutboxStatsReader` возвращает счётчики `pending`, `processed`, `failed` и время самого старого необработанного сообщения. Они доступны через `GET /outbox/stats`.

## 6. Retry

Задержка рассчитывается экспоненциально:

```text
min(RetryBaseDelay * 2^(attempt - 1), RetryMaxDelay)
```

Пример настроек:

```json
{
  "Outbox": {
    "PollingInterval": "00:00:01",
    "BatchSize": 50,
    "MaxAttempts": 5,
    "RetryBaseDelay": "00:00:02",
    "RetryMaxDelay": "00:01:00"
  }
}
```

`OutboxOptionsValidator` проверяет положительность интервалов, размера пачки, количества попыток и корректность задержек.

## 7. SMTP

`EmailSender` использует `System.Net.Mail.SmtpClient` и настройки из секции `Smtp`.

Для локальной разработки в `docker-compose.yml` поднят Mailpit:

- SMTP: `localhost:1025`
- Web UI: `http://localhost:8025`
- письма сохраняются в SQLite-томе `mailpit-data`

Пример:

```json
{
  "Smtp": {
    "Host": "localhost",
    "Port": 1025,
    "FromAddress": "noreply@outbox.local",
    "FromName": "OutboxPattern",
    "EnableSsl": false,
    "Timeout": "00:00:30"
  }
}
```

`SmtpOptionsValidator` проверяет обязательные поля, парность `UserName`/`Password` и положительность `Timeout`.

## 8. Регистрация зависимостей

```csharp
builder.Services.AddInfrastructure(builder.Configuration);
```

Регистрируются:

- `ApplicationDbContext`;
- `IUnitOfWork` как тот же scoped `ApplicationDbContext`;
- репозитории, writer, serializer и dispatcher;
- `IEmailSender`;
- `OutboxProcessor` как hosted service;
- настройки `Outbox` и `Smtp`.

## 9. Миграции

```powershell
dotnet ef migrations add <Name> `
  --project src/OutboxPattern.Infrastructure `
  --startup-project src/OutboxPattern.Api `
  --output-dir Persistence/Migrations
```

```powershell
dotnet ef database update `
  --project src/OutboxPattern.Infrastructure `
  --startup-project src/OutboxPattern.Api
```

## 10. Ограничения

- Гарантия доставки — at-least-once, обработчики должны быть идемпотентны.
- `SmtpClient` подходит для обучения; для production лучше MailKit.
- Обработанные сообщения не удаляются автоматически.
