using CadastroService.Core.Application.Features.Cadastrar;
using CadastroService.Core.Domain.Usuarios;

namespace CadastroService.Presentation.Web.Features.Cadastrar;

public static class CadastrarEndpoint
{
    /// <summary>Registra o pedido e responde 202: o cadastro continua em segundo plano, executado pelo worker.</summary>
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
            var state = await handler.Handle(new CadastrarCommand(request.Username, request.Email, request.Nome), ct);
            return Results.Accepted($"/cadastros/{state.CadastroId}", state);
        });
}
