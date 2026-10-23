namespace CadastroService.Core.Domain.Simulations;

/// <summary>Porta de saída (só demonstração): agenda falhas simuladas do Keycloak.</summary>
public interface ISimulatedFailures
{
    /// <summary>Faz as próximas <paramref name="quantity"/> criações no Keycloak falharem com erro técnico.</summary>
    Task Schedule(int quantity, CancellationToken ct);
}
