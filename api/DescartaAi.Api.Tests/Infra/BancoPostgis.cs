using Testcontainers.PostgreSql;

namespace DescartaAi.Api.Tests.Infra;

/// <summary>
/// PostgreSQL + PostGIS real em um contêiner descartável, criado uma vez para todos os testes
/// de integração. Requer Docker em execução.
/// </summary>
public sealed class BancoPostgis : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgis/postgis:16-3.4")
        .WithDatabase("descartaai_testes")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public Task InitializeAsync() => _container.StartAsync();

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}

[CollectionDefinition(Nome)]
public class ColecaoApi : ICollectionFixture<BancoPostgis>
{
    public const string Nome = "API com banco real";
}
