# OutboxPattern.UnitTests

## 1. Назначение

Проект `OutboxPattern.UnitTests` содержит юнит-тесты, проверяющие бизнес-правила доменной модели, сериализацию интеграционных событий и правила повторной обработки Outbox.

Тесты изолированы от базы данных, брокера сообщений и других инфраструктурных компонентов. Они проверяют поведение отдельного класса и не требуют запуска внешних сервисов.

## 2. Требования и стек

Для запуска тестов требуется .NET SDK 10.0: проект нацелен на `net10.0`.

| Компонент | Версия | Назначение |
|---|---|---|
| `xunit` | 2.9.3 | Фреймворк модульного тестирования |
| `Microsoft.NET.Test.Sdk` | 17.14.1 | Интеграция тестов с `dotnet test` |
| `xunit.runner.visualstudio` | 3.1.4 | Запуск xUnit-тестов в Visual Studio и `dotnet test` |
| `coverlet.collector` | 6.0.4 | Сбор покрытия кода |

## 3. Состав проекта

```text
tests/OutboxPattern.UnitTests/
├── Domain/
│   └── Orders/
│       └── OrderTests.cs
├── Infrastructure/
│   └── Outbox/
│       ├── IntegrationEventDispatcherTests.cs
│       ├── JsonIntegrationEventSerializerTests.cs
│       ├── OutboxMessageTests.cs
│       └── OutboxRetryPolicyTests.cs
├── OutboxPattern.UnitTests.csproj
└── README.md
```

`Domain/Orders/OrderTests.cs` содержит тесты сущности `Order`: создание заказа, валидацию входных данных и переходы между статусами.

`Infrastructure/Outbox/IntegrationEventDispatcherTests.cs` проверяет вызов зарегистрированного обработчика и ошибку при отсутствии обработчиков.

`Infrastructure/Outbox/JsonIntegrationEventSerializerTests.cs` проверяет сериализацию и восстановление интеграционных событий.

`Infrastructure/Outbox/OutboxMessageTests.cs` проверяет фиксацию ошибок, повторных попыток и завершение обработки сообщения.

`Infrastructure/Outbox/OutboxRetryPolicyTests.cs` проверяет расчёт экспоненциальной задержки и её ограничение.

Проект ссылается на `Domain`, `Application` и `Infrastructure`.

## 4. Запуск тестов

Команды выполняются из корня решения — каталога `OutboxPattern`.

Все тесты решения:

```powershell
dotnet test
```

Только проект юнит-тестов:

```powershell
dotnet test tests/OutboxPattern.UnitTests/OutboxPattern.UnitTests.csproj
```

Фильтр по классу или имени теста:

```powershell
dotnet test tests/OutboxPattern.UnitTests/OutboxPattern.UnitTests.csproj --filter "FullyQualifiedName~OrderTests"
dotnet test tests/OutboxPattern.UnitTests/OutboxPattern.UnitTests.csproj --filter "FullyQualifiedName~Create_InvalidEmail"
```

Сбор покрытия:

```powershell
dotnet test tests/OutboxPattern.UnitTests/OutboxPattern.UnitTests.csproj --collect:"XPlat Code Coverage"
```

Отчёт в формате Cobertura создаётся в каталоге `tests/OutboxPattern.UnitTests/TestResults/<идентификатор запуска>/coverage.cobertura.xml`.

## 5. Соглашения

- Тест состоит из трёх блоков: `// Arrange`, `// Act`, `// Assert`. Для проверки исключения блоки `Act` и `Assert` могут быть объединены.
- Имя теста строится по схеме `Method_Scenario_ExpectedResult`, например `Create_InvalidEmail_ThrowsArgumentException`.
- Один тест проверяет один сценарий и одну причину падения.
- Несколько наборов входных данных оформляются через `[Theory]` и `[InlineData]`.
- XML-документация в тестах не используется: назначение теста выражается его именем. Если сценарий требует пояснения, применяется краткий обычный комментарий.
- Тесты не обращаются к реальной инфраструктуре. При появлении внешних зависимостей используются тестовые двойники.

## 6. Покрытые сценарии

| Группа | Проверяемые случаи |
|---|---|
| Создание заказа | Корректные email и цена создают заказ в статусе `Pending` с заполненными полями и непустым `Id` |
| Валидация email | `null`, пустая строка, строка из пробелов и адрес без `@` вызывают `ArgumentException` |
| Длина email | Email ровно из 255 символов принимается; 256 символов вызывают `ArgumentException`; пробелы по краям удаляются до проверки длины |
| Валидация цены | Нулевая и отрицательная цена вызывают `ArgumentOutOfRangeException` |
| Подтверждение заказа | Заказ в статусе `Pending` переходит в `Confirmed` |
| Отмена заказа | Заказ в статусе `Pending` переходит в `Cancelled` |
| Запрещённые переходы | Повторное подтверждение или отмена, отмена подтверждённого заказа и подтверждение отменённого вызывают `InvalidOperationException` |
| Сериализация событий | Payload содержит данные события; событие восстанавливается по типу и JSON; некорректные аргументы отклоняются |
| Диспетчеризация событий | Зарегистрированный обработчик получает событие; ошибка обработчика пробрасывается; при отсутствии обработчиков выбрасывается `InvalidOperationException` |
| Повторная обработка Outbox | Ошибка увеличивает `Attempts`, сохраняет текст ошибки и планирует повтор; успешная обработка очищает ошибку; исчерпание попыток фиксирует окончательный провал |
| Задержка повтора | Задержка растёт экспоненциально и ограничивается максимумом; некорректные попытки и задержки отклоняются |

## 7. Границы текущего покрытия

Сейчас покрыты доменный слой, сериализация и диспетчеризация интеграционных событий, правила повторной обработки Outbox.

Не покрыты:

- обработчик команды `CreateOrderCommandHandler`;
- реализации `IOutboxWriter` и `IOrderRepository`;
- `OutboxProcessor`;
- реализация `IUnitOfWork` в `ApplicationDbContext`;
- конфигурация и сохранение `OutboxMessage` в PostgreSQL;
- интеграционная обработка сообщений процессором и повторные попытки в базе данных;
- взаимодействие с базой данных и брокером сообщений.

Интеграционные тесты следует размещать в отдельном проекте, чтобы они не смешивались с быстрыми изолированными юнит-тестами.

## 8. Добавление тестов

1. Разместить файл тестов рядом с тестируемым классом внутри соответствующего каталога предметной области.
2. Назвать тестовый метод по схеме `Method_Scenario_ExpectedResult`.
3. Оформить тест по шаблону Arrange–Act–Assert.
4. Не подключать реальную инфраструктуру и внешние сервисы.
5. Запустить `dotnet test` и убедиться, что новый тест проходит.
6. При изменении поведения обновить документацию в том же изменении.

## 9. Интеграционные тесты

Полный сценарий с HTTP API, PostgreSQL, фоновым процессором и отправкой письма проверяется в отдельном проекте [OutboxPattern.IntegrationTests](../OutboxPattern.IntegrationTests/README.md).
