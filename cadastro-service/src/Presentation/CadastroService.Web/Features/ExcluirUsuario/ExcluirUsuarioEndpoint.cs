using CadastroService.Application.Features.ExcluirUsuario;
using CadastroService.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace CadastroService.Web.Features.ExcluirUsuario;

public static class ExcluirUsuarioEndpoint
{
    /// <summary>Semantic lock: enquanto o cadastro está pendente, a exclusão é recusada.</summary>
    public static void Map(this IEndpointRouteBuilder app) =>
        app.MapDelete("/usuarios/{id}", async (string id, ExcluirUsuarioHandler handler, IUnitOfWork unitOfWork, CancellationToken ct) =>
        {
            try
            {
                if (!await handler.Handle(id, ct)) return Results.NotFound(new { erro = "nao_encontrado", mensagem = $"Usuário {id} não encontrado" });
                await unitOfWork.SaveChanges(ct);
                return Results.NoContent();
            }
            catch (DomainException e)
            {
                return Results.Conflict(new { erro = "cadastro_em_andamento", mensagem = e.Message });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Results.Conflict(new { erro = "conflito", mensagem = "O usuário mudou durante a exclusão; tente de novo" });
            }
        });
}
