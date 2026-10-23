using CadastroService.Application.Features.ConsultarCadastro;

namespace CadastroService.Web.Features.ConsultarCadastro;

public static class ConsultarCadastroEndpoint
{
    public static void Map(this IEndpointRouteBuilder app) =>
        app.MapGet("/cadastros/{id}", async (string id, ConsultarCadastroHandler handler, CancellationToken ct) =>
            await handler.Handle(id, ct) is { } state
                ? Results.Ok(state)
                : Results.NotFound(new { erro = "nao_encontrado", mensagem = $"Cadastro {id} não encontrado" }));
}
