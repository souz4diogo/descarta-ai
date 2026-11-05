using DescartaAi.Api.Dados;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace DescartaAi.Api.Tests.Infra;

/// <summary>
/// Sobe a API completa em memória, no ambiente "Testes" (comporta-se como produção:
/// sem Swagger, com limite de requisições), apontando para o banco do contêiner.
/// </summary>
public sealed class ApiFactory(string connectionString, int requisicoesPorMinuto = 10_000)
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testes");
        builder.UseSetting("ConnectionStrings:Default", connectionString);
        builder.UseSetting("Banco:MigrarAoIniciar", "true");
        builder.UseSetting("Limite:RequisicoesPorMinuto", requisicoesPorMinuto.ToString());
        builder.UseSetting("Cors:Origens", "https://descartaai.com.br");
    }

    /// <summary>Executa uma ação com acesso direto ao banco, para preparar cenários.</summary>
    public async Task ComBancoAsync(Func<AppDbContext, Task> acao)
    {
        await using var escopo = Services.CreateAsyncScope();
        await acao(escopo.ServiceProvider.GetRequiredService<AppDbContext>());
    }
}
