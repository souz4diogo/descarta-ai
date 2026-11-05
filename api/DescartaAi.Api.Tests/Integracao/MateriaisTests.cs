using System.Net;
using System.Net.Http.Json;
using DescartaAi.Api.Modulos.Materiais;
using DescartaAi.Api.Tests.Infra;

namespace DescartaAi.Api.Tests.Integracao;

[Collection(ColecaoApi.Nome)]
public sealed class MateriaisTests(BancoPostgis banco) : IDisposable
{
    private readonly ApiFactory _api = new(banco.ConnectionString);
    private HttpClient Cliente => _api.CreateClient();

    public void Dispose() => _api.Dispose();

    [Fact]
    public async Task Lista_as_categorias_da_carga_inicial_em_ordem_alfabetica()
    {
        var categorias = await Cliente.GetFromJsonAsync<List<CategoriaDto>>("/v1/categorias");

        Assert.Equal(7, categorias!.Count);
        // Ordem alfabética do português: "Óleo" fica entre "Medicamentos" e "Pilhas".
        var portugues = StringComparer.Create(new System.Globalization.CultureInfo("pt-BR"), ignoreCase: true);
        Assert.Equal(categorias.Select(c => c.Nome).Order(portugues), categorias.Select(c => c.Nome));
    }

    [Theory]
    [InlineData("remedio", "Medicamentos vencidos")]   // sem acento, pelo sinônimo "remédio"
    [InlineData("OLEO", "Óleo de cozinha usado")]       // maiúsculas e sem acento
    [InlineData("colchão", "Móveis e colchões")]        // sinônimo
    [InlineData("fluores", "Lâmpada fluorescente")]     // trecho do nome
    public async Task Busca_ignora_acentos_e_maiusculas_e_considera_sinonimos(string busca, string esperado)
    {
        var materiais = await Buscar($"busca={Uri.EscapeDataString(busca)}");

        Assert.Contains(materiais, m => m.Nome == esperado);
    }

    [Theory]
    [InlineData("%")]
    [InlineData("_")]
    public async Task Curingas_do_LIKE_sao_tratados_como_texto(string busca)
    {
        var materiais = await Buscar($"busca={Uri.EscapeDataString(busca)}");

        Assert.Empty(materiais);
    }

    [Fact]
    public async Task Busca_acima_de_60_caracteres_retorna_400()
    {
        var resposta = await Cliente.GetAsync($"/v1/materiais?busca={new string('a', 61)}");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("application/problem+json", resposta.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Filtra_por_categoria()
    {
        var categorias = await Cliente.GetFromJsonAsync<List<CategoriaDto>>("/v1/categorias");
        var lampadas = categorias!.Single(c => c.Nome == "Lâmpadas");

        var materiais = await Buscar($"categoriaId={lampadas.Id}");

        Assert.Equal(2, materiais.Count);
        Assert.All(materiais, m => Assert.Equal(lampadas.Id, m.CategoriaId));
    }

    [Fact]
    public async Task Detalhe_traz_instrucoes_e_cuidados()
    {
        var pilha = (await Buscar("busca=pilha")).Single();

        var detalhe = await Cliente.GetFromJsonAsync<MaterialDetalheDto>($"/v1/materiais/{pilha.Id}");

        Assert.True(detalhe!.Perigoso);
        Assert.False(string.IsNullOrWhiteSpace(detalhe.Instrucoes));
        Assert.False(string.IsNullOrWhiteSpace(detalhe.Cuidados));
    }

    [Fact]
    public async Task Material_inexistente_retorna_404_no_padrao_RFC_7807()
    {
        var resposta = await Cliente.GetAsync("/v1/materiais/999999");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Equal("application/problem+json", resposta.Content.Headers.ContentType?.MediaType);
    }

    private async Task<List<MaterialResumoDto>> Buscar(string consulta) =>
        (await Cliente.GetFromJsonAsync<List<MaterialResumoDto>>($"/v1/materiais?{consulta}"))!;
}
