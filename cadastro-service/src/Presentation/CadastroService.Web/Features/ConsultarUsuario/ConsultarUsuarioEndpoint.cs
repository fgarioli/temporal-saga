using CadastroService.Application.Features.ConsultarUsuario;

namespace CadastroService.Web.Features.ConsultarUsuario;

public static class ConsultarUsuarioEndpoint
{
    public static void Map(this IEndpointRouteBuilder app) =>
        app.MapGet("/usuarios/{id}", async (string id, ConsultarUsuarioHandler handler, CancellationToken ct) =>
            await handler.Handle(id, ct) is { } u
                ? Results.Ok(new { u.Id, u.Username, u.Email, u.Nome, Status = u.Status.ToString(), u.KeycloakId, u.Motivo })
                : Results.NotFound(new { erro = "nao_encontrado", mensagem = $"Usuário {id} não encontrado" }));
}
