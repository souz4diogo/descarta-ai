using System.Text.Json;
using DescartaAi.Api.Dominio;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace DescartaAi.Api.Dados.Carga;

/// <summary>
/// Popula o banco vazio: catálogo de categorias e materiais e pontos de Araçatuba vindos do
/// OpenStreetMap (pontos-osm-aracatuba.json, © colaboradores do OpenStreetMap, licença ODbL).
/// Cada etapa só roda se a respectiva tabela estiver vazia.
/// </summary>
public static class CargaInicial
{
    private const string ArquivoPontos = "pontos-osm-aracatuba.json";

    public static async Task ExecutarAsync(AppDbContext db, CancellationToken ct = default)
    {
        if (!await db.Categorias.AnyAsync(ct))
        {
            db.Categorias.AddRange(CatalogoInicial.Criar());
            await db.SaveChangesAsync(ct);
        }

        if (!await db.Pontos.AnyAsync(ct))
        {
            db.Pontos.AddRange(await LerPontosOsmAsync(db, ct));
            await db.SaveChangesAsync(ct);
        }
    }

    private static async Task<IEnumerable<PontoDescarte>> LerPontosOsmAsync(AppDbContext db, CancellationToken ct)
    {
        var caminho = Path.Combine(AppContext.BaseDirectory, "Dados", "Carga", ArquivoPontos);
        await using var arquivo = File.OpenRead(caminho);
        var itens = await JsonSerializer.DeserializeAsync<List<PontoOsm>>(
            arquivo, new JsonSerializerOptions(JsonSerializerDefaults.Web), ct) ?? [];

        var idsPorNome = await db.Materiais.ToDictionaryAsync(m => m.Nome, m => m.Id, ct);
        var geometria = new GeometryFactory(new PrecisionModel(), Srid.Wgs84);

        return itens.Select(i => new PontoDescarte
        {
            Nome = i.Nome, Endereco = i.Endereco, Cidade = i.Cidade, Uf = i.Uf, Cep = i.Cep,
            Localizacao = geometria.CreatePoint(new Coordinate(i.Lng, i.Lat)),
            Status = StatusPonto.Aprovado,
            AprovadoEm = DateTime.UtcNow,
            Materiais = i.Materiais.Where(idsPorNome.ContainsKey)
                .Select(nome => new PontoMaterial { MaterialId = idsPorNome[nome] })
                .ToList()
        });
    }

    private record PontoOsm(string OsmId, string Nome, string Endereco, string Cidade, string Uf,
        string? Cep, double Lat, double Lng, string[] Materiais);
}
