namespace DescartaAi.Api.Modulos;

public record CategoriaDto(int Id, string Nome, string? Icone, string? Cor);

public record MaterialResumoDto(int Id, int CategoriaId, string Categoria, string Nome, bool Perigoso);

public record MaterialDetalheDto(
    int Id, int CategoriaId, string Categoria, string Nome,
    string Instrucoes, string? Cuidados, string[]? Sinonimos, bool Perigoso);

public record PontoProximoDto(
    Guid Id, string Nome, string Endereco, double DistanciaMetros,
    double Lat, double Lng, string[] Materiais);

public record PontoDetalheDto(
    Guid Id, string Nome, string Endereco, string Cidade, string Uf, string? Cep,
    string? Telefone, string? Horario, double Lat, double Lng, string[] Materiais);
