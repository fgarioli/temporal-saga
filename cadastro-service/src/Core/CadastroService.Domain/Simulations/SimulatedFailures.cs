namespace CadastroService.Core.Domain.Simulations;

/// <summary>
/// Só para demonstração: quantas das próximas chamadas a um alvo (uma operação do Keycloak) devem falhar com erro técnico, para
/// mostrar que o Temporal resolve falhas temporárias com novas tentativas. Existe um registro por alvo; o id é o nome do alvo.
/// </summary>
public sealed class SimulatedFailures
{
    public const string KeycloakCreate = "keycloak-create";
    public const string KeycloakEnable = "keycloak-enable";

    private static readonly string[] Alvos = [KeycloakCreate, KeycloakEnable];

    public string Id { get; }
    public int Remaining { get; private set; }

    private SimulatedFailures(string id, int remaining)
    {
        Id = id;
        Remaining = remaining;
    }

    /// <summary>O registro nasce sem falhas agendadas.</summary>
    public static SimulatedFailures Create(string alvo) => new(alvo, 0);

    /// <summary>Diz se o alvo é uma operação que a simulação sabe fazer falhar.</summary>
    public static bool IsKnown(string? alvo) => alvo is not null && Alvos.Contains(alvo);

    /// <summary>Agenda mais <paramref name="quantity"/> falhas, somando às que ainda restam.</summary>
    public void Schedule(int quantity) => Remaining += quantity;

    /// <summary>Gasta uma falha agendada, se houver. Devolve se a chamada deve falhar.</summary>
    public bool TryConsume()
    {
        if (Remaining <= 0) return false;
        Remaining--;
        return true;
    }
}
