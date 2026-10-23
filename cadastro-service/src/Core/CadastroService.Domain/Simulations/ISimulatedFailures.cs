namespace CadastroService.Core.Domain.Simulations;

/// <summary>Porta de saída (só demonstração): agenda falhas simuladas do Keycloak.</summary>
public interface ISimulatedFailures
{
    /// <summary>Faz as próximas <paramref name="quantity"/> chamadas ao <paramref name="alvo"/> falharem com erro técnico.</summary>
    Task Schedule(string alvo, int quantity, CancellationToken ct);
}
