using DescartaAi.Api.Dados;
using DescartaAi.Api.Dominio;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace DescartaAi.Api.Modulos;

public static class PontosEndpoints
{
    private const double RaioPadrao = 10_000;
    private const double RaioMaximo = 50_000;
    private static readonly GeometryFactory Fabrica = new(new PrecisionModel(), 4326);

    public static void MapPontos(this IEndpointRouteBuilder app)
    {
        app.MapGet("/pontos/proximos", async (double? lat, double? lng, double? raio, int? materialId,
            AppDbContext db, CancellationToken ct) =>
        {
            var erros = new Dictionary<string, string[]>();
            if (lat is null or < -90 or > 90) erros["lat"] = ["Informe uma latitude entre -90 e 90."];
            if (lng is null or < -180 or > 180) erros["lng"] = ["Informe uma longitude entre -180 e 180."];
            if (raio is <= 0 or > RaioMaximo) erros["raio"] = [$"O raio deve estar entre 1 e {RaioMaximo:0} metros."];
            if (erros.Count > 0) return Results.ValidationProblem(erros);

            var origem = Fabrica.CreatePoint(new Coordinate(lng!.Value, lat!.Value));
            var r = raio ?? RaioPadrao;

            var q = db.Pontos.AsNoTracking()
                .Where(p => p.Status == StatusPonto.Aprovado && p.Localizacao.IsWithinDistance(origem, r));
            if (materialId is not null) q = q.Where(p => p.Materiais.Any(pm => pm.MaterialId == materialId));

            var lista = await q.OrderBy(p => p.Localizacao.Distance(origem))
                .Take(50)
                .Select(p => new
                {
                    p.Id, p.Nome, p.Endereco,
                    Distancia = p.Localizacao.Distance(origem),
                    p.Localizacao,
                    Materiais = p.Materiais.Select(pm => pm.Material!.Nome).OrderBy(n => n).ToList()
                })
                .ToListAsync(ct);

            return Results.Ok(lista.Select(p => new PontoProximoDto(
                p.Id, p.Nome, p.Endereco, Math.Round(p.Distancia), p.Localizacao.Y, p.Localizacao.X, [.. p.Materiais])));
        }).WithTags("Pontos");

        app.MapGet("/pontos/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
        {
            var p = await db.Pontos.AsNoTracking()
                .Where(x => x.Id == id && x.Status == StatusPonto.Aprovado)
                .Select(x => new
                {
                    x.Id, x.Nome, x.Endereco, x.Cidade, x.Uf, x.Cep, x.Telefone, x.Horario,
                    x.Localizacao,
                    Materiais = x.Materiais.Select(pm => pm.Material!.Nome).OrderBy(n => n).ToList()
                })
                .FirstOrDefaultAsync(ct);
            return p is null
                ? Results.Problem(statusCode: 404, title: "Ponto não encontrado")
                : Results.Ok(new PontoDetalheDto(p.Id, p.Nome, p.Endereco, p.Cidade, p.Uf, p.Cep,
                    p.Telefone, p.Horario, p.Localizacao.Y, p.Localizacao.X, [.. p.Materiais]));
        }).WithTags("Pontos");
    }
}
