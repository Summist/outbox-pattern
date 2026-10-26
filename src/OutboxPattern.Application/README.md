# OutboxPattern.Application

## 1. Назначение

Прикладной слой содержит сценарии использования, контракты и обработчики интеграционных событий.

Слой зависит только от `OutboxPattern.Domain`. Он не знает про EF Core, PostgreSQL, SMTP и фоновый процессор.

## 2. Структура

```text
OutboxPattern.Application/
├── Abstractions/
│   ├── CQRS/
│   ├── Notifications/
│   ├── Outbox/
│   └── Persistence/
├── EventHandlers/
│   └── Orders/
├── Events/
│   └── Orders/
├── UseCases/
│   └── CreateOrder/
└── DependencyInjection.cs
```

## 3. Контракты

| Контракт | Назначение |
|---|---|
| `ICommand<TResponse>` | Маркер команды с результатом |
| `ICommandHandler<TCommand, TResponse>` | Обработчик команды |
| `IQuery<TResponse>` | Маркер запроса |
| `IQueryHandler<TQuery, TResponse>` | Обработчик запроса |
| `IIntegrationEvent` | Контракт интеграционного события |
| `IIntegrationEventHandler<TEvent>` | Обработчик события |
| `IIntegrationEventDispatcher` | Передача события обработчикам |
| `IIntegrationEventSerializer` | Сериализация события для Outbox |
| `IOutboxWriter` | Запись события в Outbox |
| `IUnitOfWork` | Сохранение изменений |
| `IOrderRepository` | Хранение заказов |
| `IEmailSender` | Отправка email |

Реализации контрактов находятся в Infrastructure. Так прикладной слой не зависит от инфраструктуры.

## 4. Сценарий создания заказа

`CreateOrderCommandHandler`:

1. создаёт доменный заказ;
2. добавляет заказ через `IOrderRepository`;
3. сериализует `OrderCreatedIntegrationEvent` и добавляет его через `IOutboxWriter`;
4. вызывает `IUnitOfWork.SaveChangesAsync`.

Заказ и сообщение Outbox должны сохраняться в одной транзакции.

## 5. Обработчик события

`OrderCreatedIntegrationEventHandler` реагирует на создание заказа и отправляет письмо покупателю через `IEmailSender`.

Обработчик не знает, как именно отправляется письмо: SMTP, Mailpit или тестовый двойник определяются в Infrastructure.

## 6. Регистрация зависимостей

```csharp
builder.Services.AddApplication();
```

`AddApplication()` регистрирует внутренние обработчики команд и событий, не раскрывая их как публичный API.

## 7. Правила слоя

- Не ссылаться на EF Core, Npgsql, ASP.NET Core и `BackgroundService`.
- Не содержать SQL и настройки SMTP.
- Не создавать `DbContext` напрямую.
- Новые сценарии размещать в `UseCases/<UseCase>/`.
- Новые обработчики событий размещать в `EventHandlers/<Aggregate>/`.

## 8. Тесты

Прикладной слой тестируется юнит-тестами с поддельными реализациями `IOrderRepository`, `IOutboxWriter`, `IUnitOfWork` и `IEmailSender`.
