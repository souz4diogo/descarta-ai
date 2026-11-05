using DescartaAi.Api.Dados;
using Microsoft.EntityFrameworkCore;

namespace DescartaAi.Api.Modulos;

public static class MateriaisEndpoints
{
    public static void MapMateriais(this IEndpointRouteBuilder app)
    {
        app.MapGet("/categorias", async (AppDbContext db, CancellationToken ct) =>
            await db.Categorias.AsNoTracking().OrderBy(c => c.Nome)
                .Select(c => new CategoriaDto(c.Id, c.Nome, c.Icone, c.Cor))
                .ToListAsync(ct))
            .WithTags("Materiais");

        app.MapGet("/materiais", async (string? busca, int? categoriaId, AppDbContext db, CancellationToken ct) =>
        {
            var q = db.Materiais.AsNoTracking().AsQueryable();
            if (categoriaId is not null) q = q.Where(m => m.CategoriaId == categoriaId);
            if (!string.IsNullOrWhiteSpace(busca))
            {
                var padrao = $"%{busca.Trim()}%";
                q = q.Where(m => EF.Functions.ILike(EF.Functions.Unaccent(m.Nome), EF.Functions.Unaccent(padrao))
                    || (m.Sinonimos != null && m.Sinonimos.Any(s =>
                        EF.Functions.ILike(EF.Functions.Unaccent(s), EF.Functions.Unaccent(padrao)))));
            }
            return await q.OrderBy(m => m.Nome)
                .Select(m => new MaterialResumoDto(m.Id, m.CategoriaId, m.Categoria!.Nome, m.Nome, m.Perigoso))
                .ToListAsync(ct);
        }).WithTags("Materiais");

        app.MapGet("/materiais/{id:int}", async (int id, AppDbContext db, CancellationToken ct) =>
        {
            var m = await db.Materiais.AsNoTracking().Where(x => x.Id == id)
                .Select(x => new MaterialDetalheDto(x.Id, x.CategoriaId, x.Categoria!.Nome, x.Nome,
                    x.Instrucoes, x.Cuidados, x.Sinonimos, x.Perigoso))
                .FirstOrDefaultAsync(ct);
            return m is null ? Results.Problem(statusCode: 404, title: "Material não encontrado") : Results.Ok(m);
        }).WithTags("Materiais");
    }
}
