using DescartaAi.Api.Dados;
using Microsoft.EntityFrameworkCore;

namespace DescartaAi.Api.Modulos.Materiais;

public interface IMateriaisServico
{
    Task<IReadOnlyList<CategoriaDto>> ListarCategoriasAsync(CancellationToken ct);
    Task<IReadOnlyList<MaterialResumoDto>> BuscarAsync(string? busca, int? categoriaId, CancellationToken ct);
    Task<MaterialDetalheDto?> ObterAsync(int id, CancellationToken ct);
}

public class MateriaisServico(AppDbContext db) : IMateriaisServico
{
    public const int TamanhoMaximoBusca = 60;

    public async Task<IReadOnlyList<CategoriaDto>> ListarCategoriasAsync(CancellationToken ct) =>
        await db.Categorias.AsNoTracking()
            .OrderBy(c => c.Nome)
            .Select(c => new CategoriaDto(c.Id, c.Nome, c.Icone, c.Cor))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<MaterialResumoDto>> BuscarAsync(string? busca, int? categoriaId, CancellationToken ct)
    {
        var consulta = db.Materiais.AsNoTracking();

        if (categoriaId is not null)
            consulta = consulta.Where(m => m.CategoriaId == categoriaId);

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var padrao = $"%{EscaparCuringas(busca.Trim())}%";
            consulta = consulta.Where(m =>
                EF.Functions.ILike(EF.Functions.Unaccent(m.Nome), EF.Functions.Unaccent(padrao))
                || (m.Sinonimos != null && m.Sinonimos.Any(s =>
                    EF.Functions.ILike(EF.Functions.Unaccent(s), EF.Functions.Unaccent(padrao)))));
        }

        return await consulta
            .OrderBy(m => m.Nome)
            .Select(m => new MaterialResumoDto(m.Id, m.CategoriaId, m.Categoria!.Nome, m.Nome, m.Perigoso))
            .ToListAsync(ct);
    }

    public Task<MaterialDetalheDto?> ObterAsync(int id, CancellationToken ct) =>
        db.Materiais.AsNoTracking()
            .Where(m => m.Id == id)
            .Select(m => new MaterialDetalheDto(m.Id, m.CategoriaId, m.Categoria!.Nome, m.Nome,
                m.Instrucoes, m.Cuidados, m.Sinonimos, m.Perigoso))
            .FirstOrDefaultAsync(ct);

    /// <summary>Impede que % e _ digitados pelo usuário virem curingas do LIKE.</summary>
    private static string EscaparCuringas(string texto) =>
        texto.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_");
}
