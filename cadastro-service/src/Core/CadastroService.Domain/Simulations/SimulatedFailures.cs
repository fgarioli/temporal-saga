namespace CadastroService.Core.Domain.Simulations;

/// <summary>
/// Só para demonstração: quantas das próximas criações no Keycloak devem falhar com erro técnico, para mostrar que o Temporal
/// resolve falhas temporárias com novas tentativas. Existe um único registro (<see cref="SingletonId"/>).
/// </summary>
public sealed class SimulatedFailures
{
    public const int SingletonId = 1;

    public int Id { get; }
    public int Remaining { get; private set; }

    private SimulatedFailures(int id, int remaining)
    {
        Id = id;
        Remaining = remaining;
    }

    /// <summary>O registro nasce sem falhas agendadas.</summary>
    public static SimulatedFailures Create() => new(SingletonId, 0);

    /// <summary>Agenda mais <paramref name="quantity"/> falhas, somando às que ainda restam.</summary>
    public void Schedule(int quantity) => Remaining += quantity;

    /// <summary>Gasta uma falha agendada, se houver. Devolve se a criação deve falhar.</summary>
    public bool TryConsume()
    {
        if (Remaining <= 0) return false;
        Remaining--;
        return true;
    }
}
