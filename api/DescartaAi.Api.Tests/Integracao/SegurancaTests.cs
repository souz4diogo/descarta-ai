using System.Net;
using DescartaAi.Api.Tests.Infra;

namespace DescartaAi.Api.Tests.Integracao;

[Collection(ColecaoApi.Nome)]
public sealed class SegurancaTests(BancoPostgis banco)
{
    [Fact]
    public async Task Respostas_trazem_cabecalhos_de_seguranca()
    {
        using var api = new ApiFactory(banco.ConnectionString);

        var resposta = await api.CreateClient().GetAsync("/v1/categorias");

        Assert.Equal("nosniff", resposta.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", resposta.Headers.GetValues("X-Frame-Options").Single());
        Assert.Equal("no-referrer", resposta.Headers.GetValues("Referrer-Policy").Single());
    }

    [Fact]
    public async Task Swagger_fica_desligado_fora_do_ambiente_de_desenvolvimento()
    {
        using var api = new ApiFactory(banco.ConnectionString);

        var resposta = await api.CreateClient().GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task CORS_libera_apenas_a_origem_configurada()
    {
        using var api = new ApiFactory(banco.ConnectionString);
        var cliente = api.CreateClient();

        var permitida = await GetComOrigem(cliente, "https://descartaai.com.br");
        var estranha = await GetComOrigem(cliente, "https://site-malicioso.example");

        Assert.Equal("https://descartaai.com.br", permitida.Headers.GetValues("Access-Control-Allow-Origin").Single());
        Assert.False(estranha.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task Excesso_de_requisicoes_do_mesmo_IP_retorna_429()
    {
        using var api = new ApiFactory(banco.ConnectionString, requisicoesPorMinuto: 3);
        var cliente = api.CreateClient();

        var codigos = new List<HttpStatusCode>();
        for (var i = 0; i < 5; i++)
            codigos.Add((await cliente.GetAsync("/v1/categorias")).StatusCode);

        Assert.Equal(3, codigos.Count(c => c == HttpStatusCode.OK));
        Assert.Equal(2, codigos.Count(c => c == HttpStatusCode.TooManyRequests));
    }

    private static Task<HttpResponseMessage> GetComOrigem(HttpClient cliente, string origem)
    {
        var requisicao = new HttpRequestMessage(HttpMethod.Get, "/v1/categorias");
        requisicao.Headers.Add("Origin", origem);
        return cliente.SendAsync(requisicao);
    }
}
