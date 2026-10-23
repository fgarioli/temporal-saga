using CadastroService.Core.Domain.Simulations;

namespace CadastroService.Core.Application.Features.SimularFalhas;

/// <summary>Só em demonstração: faz o Keycloak "falhar" nas próximas N chamadas ao alvo (criar ou habilitar a identidade).</summary>
public sealed class SimularFalhasHandler(ISimulatedFailures failures)
{
    public Task Handle(string alvo, int quantity, CancellationToken ct) => failures.Schedule(alvo, quantity, ct);
}
