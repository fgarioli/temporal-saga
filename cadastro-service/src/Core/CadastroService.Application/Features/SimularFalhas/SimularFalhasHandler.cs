using CadastroService.Core.Domain.Simulations;

namespace CadastroService.Core.Application.Features.SimularFalhas;

/// <summary>Só em demonstração: faz o Keycloak "falhar" nas próximas N criações.</summary>
public sealed class SimularFalhasHandler(ISimulatedFailures failures)
{
    public Task Handle(int quantity, CancellationToken ct) => failures.Schedule(quantity, ct);
}
