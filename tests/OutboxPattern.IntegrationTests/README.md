# OutboxPattern.IntegrationTests

## 1. Назначение

Интеграционные тесты проверяют полный сценарий через HTTP API, реальный PostgreSQL и фоновый `OutboxProcessor`.

## 2. Требования

- .NET SDK 10.0
- Запущенный Docker

Проверить Docker:

```powershell
docker info
```

## 3. Что используется

- `Microsoft.AspNetCore.Mvc.Testing` — запуск API в памяти.
- `Testcontainers.PostgreSql` — PostgreSQL в Docker.
- Реальная EF Core-миграция.
- Поддельный `IEmailSender` для проверки отправки без SMTP.

## 4. Покрытые сценарии

| Сценарий | Проверки |
|---|---|
| Создание заказа | `POST /orders` возвращает `201`, заказ сохранён в PostgreSQL |
| Outbox | Создаётся ровно одно сообщение вместе с заказом |
| Обработка | Фоновый процессор вызывает обработчик, письмо попадает в fake-sender, сообщение получает `ProcessedAtUtc` |
| Отсутствие дублей | После успешного сценария доставлено ровно одно письмо |
| Временная ошибка SMTP | Сообщение не теряется, выполняется retry, после успеха доставлено одно письмо |
| Окончательный провал | После `MaxAttempts` выставляется `FailedAtUtc`, сообщение остаётся в БД |
| Падение и рестарт API | Сообщение сохраняется, новый экземпляр API обрабатывает его и доставляет ровно одно письмо |
| Статистика Outbox | `/outbox/stats` показывает корректные `pending`, `processed`, `failed` |
| Swagger и OpenAPI | `/swagger/index.html` и `/openapi/v1.json` доступны, документ содержит эндпоинты |
| Невалидный email | `POST /orders` возвращает `400`, заказ и сообщение не сохраняются |

## 5. Запуск

```powershell
dotnet test tests/OutboxPattern.IntegrationTests/OutboxPattern.IntegrationTests.csproj
```

При первом запуске Testcontainers скачает образ `postgres:17-alpine`.

## 6. Структура

```text
tests/OutboxPattern.IntegrationTests/
├── Infrastructure/
│   ├── FakeEmailSender.cs
│   ├── OutboxApiFactory.cs
│   ├── PostgresCollection.cs
│   ├── PostgresFixture.cs
│   └── TestWait.cs
├── Orders/
│   ├── CreateOrderTests.cs
│   └── OutboxReliabilityTests.cs
├── OutboxPattern.IntegrationTests.csproj
└── README.md
```

`PostgresFixture` запускает один контейнер на всю коллекцию тестов.

`OutboxApiFactory` переопределяет строку подключения, настройки Outbox и SMTP, а также заменяет `IEmailSender` на тестовый двойник.

`FakeEmailSender` умеет падать заданное количество раз, считать попытки и хранить только успешно доставленные письма.

## 7. Важно

Тесты используют реальную базу и фоновый сервис, поэтому они медленнее юнит-тестов и требуют Docker. В CI Docker должен быть доступен.

Гарантия Outbox — **at-least-once**. Тесты проверяют отсутствие потерь и отсутствие дублей в штатном сценарии/retry, но при сбое между внешним side effect и сохранением отметки обработчик должен быть идемпотентным.
