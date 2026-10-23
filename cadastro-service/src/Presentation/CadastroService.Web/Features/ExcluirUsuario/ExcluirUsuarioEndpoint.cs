using CadastroService.Core.Application.Features.ExcluirUsuario;

namespace CadastroService.Presentation.Web.Features.ExcluirUsuario;

public static class ExcluirUsuarioEndpoint
{
    /// <summary>Semantic lock: enquanto o cadastro está pendente, a exclusão é recusada.</summary>
    public static void Map(this IEndpointRouteBuilder app) =>
        app.MapDelete("/usuarios/{id}", async (string id, ExcluirUsuarioHandler handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(id, ct);
            return result.Status switch
            {
                UsuarioDeletionStatus.Deleted => Results.NoContent(),
                UsuarioDeletionStatus.NotFound => Results.NotFound(new { erro = "nao_encontrado", mensagem = $"Usuário {id} não encontrado" }),
                UsuarioDeletionStatus.CadastroEmAndamento => Results.Conflict(new { erro = "cadastro_em_andamento", mensagem = result.Mensagem }),
                UsuarioDeletionStatus.Conflict => Results.Conflict(new { erro = "conflito", mensagem = "O usuário mudou durante a exclusão; tente de novo" }),
                _ => throw new InvalidOperationException($"Status de exclusão desconhecido: {result.Status}")
            };
        });
}
