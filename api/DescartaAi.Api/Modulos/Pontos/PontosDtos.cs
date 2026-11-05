namespace DescartaAi.Api.Modulos.Pontos;

public record PontoProximoDto(
    Guid Id, string Nome, string Endereco, double DistanciaMetros,
    double Lat, double Lng, string[] Materiais);

public record PontoDetalheDto(
    Guid Id, string Nome, string Endereco, string Cidade, string Uf, string? Cep,
    string? Telefone, string? Horario, double Lat, double Lng, string[] Materiais);

/// <summary>Parâmetros da busca por proximidade, com a validação das regras de entrada.</summary>
public record FiltroProximidade(double? Lat, double? Lng, double? Raio, int? MaterialId)
{
    public const double RaioPadrao = 10_000;
    public const double RaioMaximo = 50_000;

    public double RaioEfetivo => Raio ?? RaioPadrao;

    public Dictionary<string, string[]> Validar()
    {
        var erros = new Dictionary<string, string[]>();
        if (Lat is null or < -90 or > 90) erros["lat"] = ["Informe uma latitude entre -90 e 90."];
        if (Lng is null or < -180 or > 180) erros["lng"] = ["Informe uma longitude entre -180 e 180."];
        if (Raio is <= 0 or > RaioMaximo) erros["raio"] = [$"O raio deve estar entre 1 e {RaioMaximo:0} metros."];
        return erros;
    }
}
