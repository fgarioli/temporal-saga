namespace CadastroService.Infrastructure.Persistence.Entities;

/// <summary>Só para demonstração: quantas das próximas criações no Keycloak devem falhar.</summary>
public sealed class SimulatedFailuresEntity
{
    public int Id { get; set; }
    public int Remaining { get; set; }
}
