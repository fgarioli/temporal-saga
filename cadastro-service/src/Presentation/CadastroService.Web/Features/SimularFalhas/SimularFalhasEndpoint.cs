using CadastroService.Core.Application.Features.SimularFalhas;
using CadastroService.Core.Domain.Simulations;

namespace CadastroService.Presentation.Web.Features.SimularFalhas;

public static class SimularFalhasEndpoint
{
    /// <summary>Só em demonstração: faz o Keycloak "falhar" nas próximas N criações.</summary>
    public static void Map(this IEndpointRouteBuilder app) =>
        app.MapPost("/simulations/failures", async (SimulatedFailuresRequest body, SimularFalhasHandler handler, CancellationToken ct) =>
        {
            await handler.Handle(body.Quantity, ct);
            return Results.NoContent();
        });
}
