namespace DescartaAi.Api.Modulos.Materiais;

public record CategoriaDto(int Id, string Nome, string? Icone, string? Cor);

public record MaterialResumoDto(int Id, int CategoriaId, string Categoria, string Nome, bool Perigoso);

public record MaterialDetalheDto(
    int Id, int CategoriaId, string Categoria, string Nome,
    string Instrucoes, string? Cuidados, string[]? Sinonimos, bool Perigoso);
