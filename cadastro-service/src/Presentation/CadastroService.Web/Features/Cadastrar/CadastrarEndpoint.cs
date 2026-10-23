using CadastroService.Core.Application.Features.Cadastrar;
using CadastroService.Core.Domain.Usuarios;

namespace CadastroService.Presentation.Web.Features.Cadastrar;

public static class CadastrarEndpoint
{
    /// <summary>
    /// Cria o usuário pendente e registra o pedido, e responde 202: o cadastro continua em segundo plano, executado pelo worker.
    /// Username já em uso: 409, sem gravar nada.
    /// </summary>
    public static void Map(this IEndpointRouteBuilder app) =>
        app.MapPost("/cadastros", async (CadastroRequest body, CadastrarHandler handler, CancellationToken ct) =>
        {
            NovoCadastro request;
            try
            {
                request = NovoCadastro.Create(body.Username, body.Email, body.Nome);
            }
            catch (ArgumentException e)
            {
                return Results.BadRequest(new { erro = "invalido", mensagem = e.Message });
            }
            var result = await handler.Handle(new CadastrarCommand(request.Username, request.Email, request.Nome), ct);
            return result switch
            {
                CadastroCreationResult.Created created => Results.Accepted($"/cadastros/{created.State.CadastroId}", created.State),
                CadastroCreationResult.UsernameEmUso emUso => Results.Conflict(new { erro = "username_em_uso", mensagem = emUso.Mensagem }),
                _ => throw new InvalidOperationException($"Resultado de cadastro desconhecido: {result}")
            };
        });
}
