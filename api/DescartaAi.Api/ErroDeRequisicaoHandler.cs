using Microsoft.AspNetCore.Diagnostics;

namespace DescartaAi.Api;

/// <summary>Converte parâmetros mal formatados (ex.: lat=abc) em 400 no padrão RFC 7807.</summary>
public class ErroDeRequisicaoHandler(IProblemDetailsService problemas) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
    {
        if (ex is not BadHttpRequestException)
            return false;

        ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
        return await problemas.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = ctx,
            Exception = ex,
            ProblemDetails = { Status = 400, Title = "Dados inválidos", Detail = "Algum parâmetro está em formato inválido." }
        });
    }
}
