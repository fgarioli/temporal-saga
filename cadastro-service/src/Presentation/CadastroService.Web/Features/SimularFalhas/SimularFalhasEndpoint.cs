using CadastroService.Core.Application.Features.SimularFalhas;
using CadastroService.Core.Domain.Simulations;

namespace CadastroService.Presentation.Web.Features.SimularFalhas;

public static class SimularFalhasEndpoint
{
    /// <summary>Só em demonstração: faz o Keycloak "falhar" nas próximas N chamadas ao alvo (padrão: keycloak-create).</summary>
    public static void Map(this IEndpointRouteBuilder app) =>
        app.MapPost("/simulations/failures", async (SimulatedFailuresRequest body, SimularFalhasHandler handler, CancellationToken ct) =>
        {
            var alvo = body.Alvo ?? SimulatedFailures.KeycloakCreate;
            if (!SimulatedFailures.IsKnown(alvo))
                return Results.BadRequest(new { erro = "invalido", mensagem = $"alvo deve ser {SimulatedFailures.KeycloakCreate} ou {SimulatedFailures.KeycloakEnable}" });
            await handler.Handle(alvo, body.Quantity, ct);
            return Results.NoContent();
        });
}
