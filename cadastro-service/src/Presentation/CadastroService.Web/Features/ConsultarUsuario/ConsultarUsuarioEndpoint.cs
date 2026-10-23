using CadastroService.Core.Application.Features.ConsultarUsuario;
using CadastroService.Presentation.Web.Common;

namespace CadastroService.Presentation.Web.Features.ConsultarUsuario;

public static class ConsultarUsuarioEndpoint
{
    public static void Map(this IEndpointRouteBuilder app) =>
        app.MapGet("/usuarios/{id}", async (string id, ConsultarUsuarioHandler handler, CancellationToken ct) =>
            await handler.Handle(id, ct) is { } usuario
                ? Results.Ok(UsuarioResponse.From(usuario))
                : Results.NotFound(new { erro = "nao_encontrado", mensagem = $"Usuário {id} não encontrado" }));
}
