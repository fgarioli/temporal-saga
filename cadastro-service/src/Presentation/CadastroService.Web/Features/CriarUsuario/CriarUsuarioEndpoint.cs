using CadastroService.Core.Application.Features.CriarUsuario;
using CadastroService.Core.Domain.Usuarios;
using CadastroService.Presentation.Web.Common;

namespace CadastroService.Presentation.Web.Features.CriarUsuario;

public static class CriarUsuarioEndpoint
{
    /// <summary>
    /// Cria o usuário Pendente e desabilitado e responde 202: o provisionamento continua em segundo plano, executado pelo worker.
    /// Username já em uso: 409, sem gravar nada.
    /// </summary>
    public static void Map(this IEndpointRouteBuilder app) =>
        app.MapPost("/usuarios", async (CriarUsuarioRequest body, CriarUsuarioHandler handler, CancellationToken ct) =>
        {
            NovoUsuario request;
            try
            {
                request = NovoUsuario.Create(body.Username, body.Email, body.Nome);
            }
            catch (ArgumentException e)
            {
                return Results.BadRequest(new { erro = "invalido", mensagem = e.Message });
            }
            var result = await handler.Handle(new CriarUsuarioCommand(request.Username, request.Email, request.Nome), ct);
            return result switch
            {
                UsuarioCreationResult.Created created => Results.Accepted($"/usuarios/{created.Usuario.Id}", UsuarioResponse.From(created.Usuario)),
                UsuarioCreationResult.UsernameEmUso emUso => Results.Conflict(new { erro = "username_em_uso", mensagem = emUso.Mensagem }),
                _ => throw new InvalidOperationException($"Resultado de criação de usuário desconhecido: {result}")
            };
        });
}
