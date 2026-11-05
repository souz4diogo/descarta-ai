namespace DescartaAi.Api.Modulos.Materiais;

public static class MateriaisEndpoints
{
    public static IEndpointRouteBuilder MapMateriais(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("").WithTags("Materiais");

        grupo.MapGet("/categorias", (IMateriaisServico servico, CancellationToken ct) =>
            servico.ListarCategoriasAsync(ct));

        grupo.MapGet("/materiais", async (string? busca, int? categoriaId, IMateriaisServico servico, CancellationToken ct) =>
        {
            if (busca?.Length > MateriaisServico.TamanhoMaximoBusca)
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["busca"] = [$"Use no máximo {MateriaisServico.TamanhoMaximoBusca} caracteres."]
                });

            return Results.Ok(await servico.BuscarAsync(busca, categoriaId, ct));
        });

        grupo.MapGet("/materiais/{id:int}", async (int id, IMateriaisServico servico, CancellationToken ct) =>
            await servico.ObterAsync(id, ct) is { } material
                ? Results.Ok(material)
                : Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Material não encontrado"));

        return app;
    }
}
