# OutboxPattern.Api

## 1. Назначение

HTTP API для создания заказов и демонстрации полного сценария Transactional Outbox.

## 2. Запуск

Всё в Docker:

```powershell
docker compose up -d --build
```

Или инфраструктура в Docker, API локально:

```powershell
docker compose up -d postgres mailpit

dotnet ef database update `
  --project src/OutboxPattern.Infrastructure `
  --startup-project src/OutboxPattern.Api

dotnet run --project src/OutboxPattern.Api
```

Адрес по умолчанию: `http://localhost:5080`.

В Docker API автоматически применяет миграции, потому что в compose задано `Database__AutoMigrate=true`.

## 3. Эндпоинты

### POST /orders

Создаёт заказ, сохраняет его вместе с сообщением Outbox и возвращает идентификатор.

Запрос:

```json
{
  "customerEmail": "customer@example.com",
  "totalPrice": 1500
}
```

Успешный ответ `201 Created`:

```json
{
  "orderId": "0199f2e0-8d9b-7c3a-9f8e-3f9b4a1c2d3e"
}
```

Ошибки валидации возвращаются как `400 Bad Request` в формате `ValidationProblem`.

### GET /health

```json
{
  "status": "ok"
}
```

### GET /outbox/stats

Возвращает состояние Outbox:

```json
{
  "pending": 0,
  "processed": 1,
  "failed": 0,
  "oldestPendingAtUtc": null
}
```

## 4. Swagger и OpenAPI

Swagger UI:

```text
http://localhost:5080/swagger
```

OpenAPI-документ:

```text
http://localhost:5080/openapi/v1.json
```

Swagger UI использует встроенный OpenAPI-документ ASP.NET Core.

## 5. Конфигурация

`appsettings.json` содержит локальные значения для PostgreSQL и Mailpit.

| Секция | Назначение |
|---|---|
| `ConnectionStrings:Postgres` | Подключение к PostgreSQL |
| `Database:AutoMigrate` | Применять миграции при старте |
| `Outbox` | Настройки фонового процессора |
| `Smtp` | Настройки отправки email |

Для production значения переопределяются переменными окружения или секрет-хранилищем.

## 6. Композиция

`Program.cs`:

1. добавляет `ProblemDetails` и OpenAPI;
2. вызывает `AddApplication()`;
3. вызывает `AddInfrastructure(configuration)`;
4. при `Database:AutoMigrate=true` применяет миграции;
5. подключает Swagger UI и эндпоинты;
6. запускает hosted service `OutboxProcessor`.

Письма поддерживают как обычный текст, так и HTML: `EmailMessage.IsHtml` управляет `MailMessage.IsBodyHtml`.

## 7. Ошибки

Доменные `ArgumentException` и `ArgumentOutOfRangeException` преобразуются в `400 Bad Request`.

Необработанные исключения возвращаются через `ProblemDetails`.
