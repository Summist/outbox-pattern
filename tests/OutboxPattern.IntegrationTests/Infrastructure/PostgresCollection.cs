namespace OutboxPattern.IntegrationTests.Infrastructure;

/// <summary>
/// Коллекция тестов, использующих общий контейнер PostgreSQL.
/// </summary>
[CollectionDefinition(CollectionName)]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    /// <summary>
    /// Имя коллекции.
    /// </summary>
    public const string CollectionName = "postgres";
}
