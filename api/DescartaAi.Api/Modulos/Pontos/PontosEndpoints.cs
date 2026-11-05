namespace DescartaAi.Api.Modulos.Pontos;

public static class PontosEndpoints
{
    public static IEndpointRouteBuilder MapPontos(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/pontos").WithTags("Pontos");

        grupo.MapGet("/proximos", async (double? lat, double? lng, double? raio, int? materialId,
            IPontosServico servico, CancellationToken ct) =>
        {
            var filtro = new FiltroProximidade(lat, lng, raio, materialId);
            var erros = filtro.Validar();
            return erros.Count > 0
                ? Results.ValidationProblem(erros)
                : Results.Ok(await servico.BuscarProximosAsync(filtro, ct));
        });

        grupo.MapGet("/{id:guid}", async (Guid id, IPontosServico servico, CancellationToken ct) =>
            await servico.ObterAprovadoAsync(id, ct) is { } ponto
                ? Results.Ok(ponto)
                : Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Ponto não encontrado"));

        return app;
    }
}
