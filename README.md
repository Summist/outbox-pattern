# OutboxPattern

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-17-4169E1)
![Docker Compose](https://img.shields.io/badge/Docker%20Compose-ready-2496ED)
![Tests](https://img.shields.io/badge/tests-43%20unit%20%2B%207%20integration-brightgreen)

Production-like учебный проект с реализацией паттерна **Transactional Outbox** на .NET 10.

Проект показывает полный путь события:

```text
HTTP → команда → заказ + OutboxMessage в одной транзакции
     → фоновый процессор → обработчик события → email
```

## Содержание

- [О проекте](#о-проекте)
- [Возможности](#возможности)
- [Архитектура](#архитектура)
- [Как работает Outbox](#как-работает-outbox)
- [Технологии](#технологии)
- [Быстрый старт](#быстрый-старт)
- [API](#api)
- [Конфигурация](#конфигурация)
- [Надёжность и гарантии](#надёжность-и-гарантии)
- [Тесты](#тесты)
- [Миграции](#миграции)
- [Структура решения](#структура-решения)
- [Документация по проектам](#документация-по-проектам)
- [Ограничения и дальнейшие шаги](#ограничения-и-дальнейшие-шаги)

## О проекте

`OutboxPattern` — пример того, как надёжно публиковать интеграционные события без потери данных между сохранением бизнес-сущности и отправкой сообщения.

Заказ и событие `OrderCreatedIntegrationEvent` сохраняются в PostgreSQL в одной транзакции. Фоновый `OutboxProcessor` читает необработанные сообщения, вызывает зарегистрированные обработчики и отправляет email. При ошибке сообщение не теряется: выполняются повторные попытки, а после исчерпания лимита фиксируется окончательный провал.

## Возможности

- Clean Architecture-подобное разделение на Domain, Application, Infrastructure и API.
- PostgreSQL + EF Core с миграциями.
- Атомарное сохранение заказа и Outbox-сообщения.
- Фоновый процессор с polling, exponential backoff и dead-letter.
- Кэшируемый диспетчер событий на скомпилированных делегатах.
- Отправка email через SMTP с поддержкой обычного текста и HTML.
- Swagger UI и OpenAPI.
- Mailpit для локального просмотра писем.
- pgAdmin для работы с PostgreSQL через web.
- Dockerfile и Docker Compose для запуска всего стека.
- Юнит-тесты и интеграционные тесты с PostgreSQL в Testcontainers.
- Проверки отсутствия потерь, повторной доставки и обработки после рестарта API.

## Архитектура

```text
OutboxPattern.Api
  ├── OutboxPattern.Application
  │     └── OutboxPattern.Domain
  └── OutboxPattern.Infrastructure
        ├── OutboxPattern.Application
        └── OutboxPattern.Domain
```

| Проект | Назначение |
|---|---|
| `OutboxPattern.Domain` | Заказ и его бизнес-правила |
| `OutboxPattern.Application` | CQRS-контракты, сценарии, обработчики событий, порты |
| `OutboxPattern.Infrastructure` | EF Core, PostgreSQL, Outbox, SMTP, фоновый процессор |
| `OutboxPattern.Api` | HTTP API и композиция зависимостей |
| `OutboxPattern.UnitTests` | Быстрые изолированные тесты |
| `OutboxPattern.IntegrationTests` | Тесты с PostgreSQL в Testcontainers |

## Как работает Outbox

1. `POST /orders` вызывает `CreateOrderCommandHandler`.
2. Заказ добавляется в таблицу `Orders`.
3. Событие `OrderCreatedIntegrationEvent` сериализуется и добавляется в `OutboxMessages`.
4. `SaveChangesAsync` сохраняет заказ и сообщение в одной транзакции.
5. `OutboxProcessor` периодически выбирает необработанные сообщения.
6. `IntegrationEventDispatcher` вызывает зарегистрированные обработчики.
7. `OrderCreatedIntegrationEventHandler` отправляет письмо покупателю.
8. Сообщение помечается обработанным. При ошибке сохраняются `Attempts`, `Error` и `NextAttemptAtUtc`.

## Технологии

| Технология | Использование |
|---|---|
| .NET 10 | Платформа приложения |
| ASP.NET Core Minimal API | HTTP API |
| EF Core 10 | ORM и миграции |
| PostgreSQL 17 | Основная база данных |
| xUnit | Юнит- и интеграционные тесты |
| Testcontainers | PostgreSQL для интеграционных тестов |
| Swagger UI | Документация API |
| Mailpit | Локальный SMTP-сервер и просмотр писем |
| pgAdmin | Web-интерфейс PostgreSQL |
| Docker Compose | Запуск всего стека |

## Быстрый старт

### Требования

- .NET SDK 10.0
- Docker Desktop или другой Docker-движок
- `dotnet-ef` для локальной работы с миграциями

```powershell
dotnet tool install --global dotnet-ef
```

### Вариант 1. Всё в Docker

```powershell
docker compose up -d --build
```

Поднимаются PostgreSQL, Mailpit, pgAdmin и API. В контейнере API автоматически применяет миграции при старте.

| Сервис | Адрес |
|---|---|
| API | `http://localhost:5080` |
| Swagger UI | `http://localhost:5080/swagger` |
| OpenAPI | `http://localhost:5080/openapi/v1.json` |
| Mailpit | `http://localhost:8025` |
| pgAdmin | `http://localhost:5050` |
| Outbox stats | `http://localhost:5080/outbox/stats` |

pgAdmin:

- логин: `admin@outboxpattern.io`
- пароль: `admin`
- сервер `OutboxPattern PostgreSQL` добавляется автоматически из `servers.json`;
- база: `outbox_pattern`, пользователь: `postgres`, пароль: `postgres`;
- если pgAdmin попросит пароль сервера, введите `postgres`.

Compose-проект называется `outbox-pattern`, поэтому контейнеры и сеть имеют префикс `outbox-pattern-`. Данные хранятся в volumes `outbox-pattern_postgres-data`, `outbox-pattern_mailpit-data` и `outbox-pattern_pgadmin-data`.

### Вариант 2. Инфраструктура в Docker, API локально

```powershell
docker compose up -d postgres mailpit pgadmin

dotnet ef database update `
  --project src/OutboxPattern.Infrastructure `
  --startup-project src/OutboxPattern.Api

dotnet run --project src/OutboxPattern.Api
```

API будет доступен на `http://localhost:5080`.

### Проверка доставки

Создать заказ:

```bash
curl -X POST http://localhost:5080/orders \
  -H "Content-Type: application/json" \
  -d '{"customerEmail":"customer@example.com","totalPrice":1500}'
```

Письмо появится в Mailpit: `http://localhost:8025`.

Посмотреть состояние Outbox:

```bash
curl http://localhost:5080/outbox/stats
```

Пример ответа:

```json
{
  "pending": 0,
  "processed": 1,
  "failed": 0,
  "oldestPendingAtUtc": null
}
```

### Остановка

```powershell
docker compose down
```

Удалить контейнеры и данные:

```powershell
docker compose down -v
```

## API

| Метод | Путь | Назначение |
|---|---|---|
| `POST` | `/orders` | Создать заказ |
| `GET` | `/health` | Проверка живости API |
| `GET` | `/outbox/stats` | Статистика Outbox |
| `GET` | `/swagger` | Swagger UI |
| `GET` | `/openapi/v1.json` | OpenAPI-документ |

### POST /orders

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

### GET /outbox/stats

```json
{
  "pending": 0,
  "processed": 1,
  "failed": 0,
  "oldestPendingAtUtc": null
}
```

## Конфигурация

Основные настройки находятся в `src/OutboxPattern.Api/appsettings.json`.

| Секция | Назначение |
|---|---|
| `ConnectionStrings:Postgres` | Строка подключения к PostgreSQL |
| `Database:AutoMigrate` | Применять миграции при старте |
| `Outbox` | Интервал опроса, размер пачки, retry-политика |
| `Smtp` | Параметры SMTP-сервера |

Пример переопределения через переменные окружения:

```powershell
$env:ConnectionStrings__Postgres = "Host=localhost;Port=5432;Database=outbox_pattern;Username=postgres;Password=postgres"
$env:Smtp__Host = "localhost"
$env:Smtp__Port = "1025"
```

Пароли и секреты не следует хранить в `appsettings.json` в репозитории. Для локального запуска используются значения из `docker-compose.yml`.

## Надёжность и гарантии

- **Атомарность.** Заказ и Outbox-сообщение сохраняются одним `SaveChangesAsync` в одной транзакции.
- **Повторные попытки.** При ошибке сохраняются `Attempts`, `Error` и `NextAttemptAtUtc`; задержка растёт экспоненциально.
- **Dead-letter.** После `MaxAttempts` сообщение получает `FailedAtUtc` и больше не выбирается процессором, но остаётся в базе.
- **Устойчивость к рестарту.** Если API остановить после сохранения заказа, сообщение останется в `OutboxMessages` и будет обработано новым экземпляром.
- **Проверка дублей.** Интеграционные тесты проверяют, что в штатном сценарии и после retry доставляется ровно одно письмо.
- **At-least-once.** При сбое между внешним side effect и сохранением отметки обработки сообщение может быть обработано повторно. Обработчики должны быть идемпотентны.

## Тесты

Юнит-тесты:

```powershell
dotnet test tests/OutboxPattern.UnitTests/OutboxPattern.UnitTests.csproj
```

Интеграционные тесты требуют запущенного Docker:

```powershell
docker info
dotnet test tests/OutboxPattern.IntegrationTests/OutboxPattern.IntegrationTests.csproj
```

Все тесты решения:

```powershell
dotnet test OutboxPattern.slnx
```

Интеграционные тесты поднимают PostgreSQL в Testcontainers, применяют миграции и проверяют:

- полный сценарий `POST /orders` → Outbox → письмо → `ProcessedAtUtc`;
- временную ошибку SMTP и повторную обработку без потери сообщения;
- окончательный провал после `MaxAttempts` без удаления сообщения;
- остановку и повторный запуск API: сообщение сохраняется и обрабатывается новым экземпляром;
- отсутствие дублей письма в штатном сценарии и после retry;
- Swagger UI, OpenAPI и статистику Outbox.

Всего: 43 юнит-теста и 7 интеграционных тестов.

## Миграции

Создать новую миграцию:

```powershell
dotnet ef migrations add <Name> `
  --project src/OutboxPattern.Infrastructure `
  --startup-project src/OutboxPattern.Api `
  --output-dir Persistence/Migrations
```

Применить миграции:

```powershell
dotnet ef database update `
  --project src/OutboxPattern.Infrastructure `
  --startup-project src/OutboxPattern.Api
```

## Структура решения

```text
.
├── src/
│   ├── OutboxPattern.Api/
│   │   ├── Contracts/
│   │   ├── Endpoints/
│   │   ├── Properties/
│   │   └── Program.cs
│   ├── OutboxPattern.Application/
│   │   ├── Abstractions/
│   │   │   ├── CQRS/
│   │   │   ├── Notifications/
│   │   │   ├── Outbox/
│   │   │   └── Persistence/
│   │   ├── EventHandlers/
│   │   ├── Events/
│   │   ├── UseCases/
│   │   └── DependencyInjection.cs
│   ├── OutboxPattern.Domain/
│   │   └── Orders/
│   └── OutboxPattern.Infrastructure/
│       ├── Notifications/
│       ├── Outbox/
│       ├── Persistence/
│       │   ├── Configurations/
│       │   ├── Migrations/
│       │   └── Repositories/
│       └── DependencyInjection.cs
├── tests/
│   ├── OutboxPattern.UnitTests/
│   └── OutboxPattern.IntegrationTests/
├── Dockerfile
├── docker-compose.yml
├── servers.json
└── README.md
```

## Документация по проектам

- [Domain](src/OutboxPattern.Domain/README.md)
- [Application](src/OutboxPattern.Application/README.md)
- [Infrastructure](src/OutboxPattern.Infrastructure/README.md)
- [API](src/OutboxPattern.Api/README.md)
- [UnitTests](tests/OutboxPattern.UnitTests/README.md)
- [IntegrationTests](tests/OutboxPattern.IntegrationTests/README.md)

## Ограничения и дальнейшие шаги

- Процессор рассчитан на один экземпляр приложения. Для нескольких экземпляров нужна блокировка `FOR UPDATE SKIP LOCKED` или распределённая блокировка.
- `System.Net.Mail.SmtpClient` подходит для обучения. Для production-нагрузок лучше рассмотреть MailKit.
- Обработанные сообщения Outbox пока не удаляются. Позже можно добавить retention/cleanup.
- Для внешних side effects нужна идемпотентность: ключом может служить `OrderId`.
