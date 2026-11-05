using System.Net;
using System.Net.Http.Json;
using DescartaAi.Api.Dominio;
using DescartaAi.Api.Modulos.Materiais;
using DescartaAi.Api.Modulos.Pontos;
using DescartaAi.Api.Tests.Infra;
using NetTopologySuite.Geometries;

namespace DescartaAi.Api.Tests.Integracao;

[Collection(ColecaoApi.Nome)]
public sealed class PontosTests(BancoPostgis banco) : IDisposable
{
    private const string Centro = "lat=-21.2089&lng=-50.4329";
    private readonly ApiFactory _api = new(banco.ConnectionString);
    private HttpClient Cliente => _api.CreateClient();

    public void Dispose() => _api.Dispose();

    [Fact]
    public async Task Retorna_pontos_ordenados_do_mais_perto_ao_mais_longe()
    {
        var pontos = await Proximos($"{Centro}&raio=50000");

        Assert.NotEmpty(pontos);
        Assert.Equal(pontos.Select(p => p.DistanciaMetros).Order(), pontos.Select(p => p.DistanciaMetros));
    }

    [Fact]
    public async Task Respeita_o_raio_informado()
    {
        var todos = await Proximos($"{Centro}&raio=50000");
        var perto = await Proximos($"{Centro}&raio=2000");

        Assert.True(perto.Count < todos.Count);
        Assert.All(perto, p => Assert.True(p.DistanciaMetros <= 2000));
    }

    [Fact]
    public async Task Filtro_por_material_traz_so_pontos_que_aceitam_o_material()
    {
        var pilha = (await Cliente.GetFromJsonAsync<List<MaterialResumoDto>>("/v1/materiais?busca=pilha"))!.Single();

        var pontos = await Proximos($"{Centro}&raio=50000&materialId={pilha.Id}");

        Assert.NotEmpty(pontos);
        Assert.All(pontos, p => Assert.Contains(pilha.Nome, p.Materiais));
    }

    [Fact]
    public async Task Detalhe_de_ponto_aprovado_traz_coordenadas_e_materiais()
    {
        var primeiro = (await Proximos($"{Centro}&raio=50000")).First();

        var detalhe = await Cliente.GetFromJsonAsync<PontoDetalheDto>($"/v1/pontos/{primeiro.Id}");

        Assert.Equal(primeiro.Lat, detalhe!.Lat, precision: 6);
        Assert.Equal(primeiro.Lng, detalhe.Lng, precision: 6);
        Assert.NotEmpty(detalhe.Materiais);
    }

    [Fact]
    public async Task Ponto_pendente_de_aprovacao_nao_e_publico()
    {
        var pendente = new PontoDescarte
        {
            Nome = "Ponto pendente de teste", Endereco = "Rua de Teste, 1", Cidade = "Araçatuba", Uf = "SP",
            Localizacao = new GeometryFactory(new PrecisionModel(), Srid.Wgs84).CreatePoint(new Coordinate(-50.4329, -21.2089)),
            Status = StatusPonto.Pendente
        };
        await _api.ComBancoAsync(async db => { db.Pontos.Add(pendente); await db.SaveChangesAsync(); });

        try
        {
            var pontos = await Proximos($"{Centro}&raio=1000");
            var detalhe = await Cliente.GetAsync($"/v1/pontos/{pendente.Id}");

            Assert.DoesNotContain(pontos, p => p.Id == pendente.Id);
            Assert.Equal(HttpStatusCode.NotFound, detalhe.StatusCode);
        }
        finally
        {
            await _api.ComBancoAsync(async db => { db.Pontos.Remove(pendente); await db.SaveChangesAsync(); });
        }
    }

    [Theory]
    [InlineData("lng=-50.4")]                         // sem latitude
    [InlineData("lat=95&lng=-50.4")]                  // latitude impossível
    [InlineData($"{Centro}&raio=0")]
    [InlineData($"{Centro}&raio=50001")]
    [InlineData("lat=abc&lng=-50.4")]                 // formato inválido
    public async Task Parametros_invalidos_retornam_400(string consulta)
    {
        var resposta = await Cliente.GetAsync($"/v1/pontos/proximos?{consulta}");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("application/problem+json", resposta.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Ponto_inexistente_retorna_404()
    {
        var resposta = await Cliente.GetAsync($"/v1/pontos/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    private async Task<List<PontoProximoDto>> Proximos(string consulta) =>
        (await Cliente.GetFromJsonAsync<List<PontoProximoDto>>($"/v1/pontos/proximos?{consulta}"))!;
}
