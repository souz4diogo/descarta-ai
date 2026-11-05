using DescartaAi.Api.Dados;
using DescartaAi.Api.Dominio;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace DescartaAi.Api.Modulos.Pontos;

public interface IPontosServico
{
    Task<IReadOnlyList<PontoProximoDto>> BuscarProximosAsync(FiltroProximidade filtro, CancellationToken ct);
    Task<PontoDetalheDto?> ObterAprovadoAsync(Guid id, CancellationToken ct);
}

public class PontosServico(AppDbContext db) : IPontosServico
{
    private const int LimiteResultados = 50;
    private static readonly GeometryFactory Geometria = new(new PrecisionModel(), Srid.Wgs84);

    public async Task<IReadOnlyList<PontoProximoDto>> BuscarProximosAsync(FiltroProximidade filtro, CancellationToken ct)
    {
        var origem = Geometria.CreatePoint(new Coordinate(filtro.Lng!.Value, filtro.Lat!.Value));
        var raio = filtro.RaioEfetivo;

        var consulta = Aprovados()
            .Where(p => p.Localizacao.IsWithinDistance(origem, raio));
        if (filtro.MaterialId is { } materialId)
            consulta = consulta.Where(p => p.Materiais.Any(pm => pm.MaterialId == materialId));

        var linhas = await consulta
            .OrderBy(p => p.Localizacao.Distance(origem))
            .Take(LimiteResultados)
            .Select(p => new
            {
                p.Id, p.Nome, p.Endereco, p.Localizacao,
                Distancia = p.Localizacao.Distance(origem),
                Materiais = p.Materiais.Select(pm => pm.Material!.Nome).OrderBy(n => n).ToArray()
            })
            .ToListAsync(ct);

        // Coordenadas lidas do Point em memória: o PostGIS não tem ST_X/ST_Y para geography.
        return linhas
            .Select(p => new PontoProximoDto(p.Id, p.Nome, p.Endereco, Math.Round(p.Distancia),
                p.Localizacao.Y, p.Localizacao.X, p.Materiais))
            .ToList();
    }

    public async Task<PontoDetalheDto?> ObterAprovadoAsync(Guid id, CancellationToken ct)
    {
        var p = await Aprovados()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id, x.Nome, x.Endereco, x.Cidade, x.Uf, x.Cep, x.Telefone, x.Horario, x.Localizacao,
                Materiais = x.Materiais.Select(pm => pm.Material!.Nome).OrderBy(n => n).ToArray()
            })
            .FirstOrDefaultAsync(ct);

        return p is null ? null : new PontoDetalheDto(p.Id, p.Nome, p.Endereco, p.Cidade, p.Uf, p.Cep,
            p.Telefone, p.Horario, p.Localizacao.Y, p.Localizacao.X, p.Materiais);
    }

    /// <summary>Somente pontos aprovados são públicos.</summary>
    private IQueryable<PontoDescarte> Aprovados() =>
        db.Pontos.AsNoTracking().Where(p => p.Status == StatusPonto.Aprovado);
}
