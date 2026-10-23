using CadastroService.Application.Common.Cadastros;

namespace CadastroService.Application.Common.Ports;

/// <summary>
/// Porta de saída: onde o cadastro (o pedido e o andamento da saga) fica gravado. Nenhum método grava sozinho;
/// quem chamou faz o commit pelo IUnitOfWork.
/// </summary>
public interface ICadastroRepository
{
    /// <summary>Grava o pedido no estado Pendente.</summary>
    void Add(CadastroData data);

    Task<CadastroState?> Get(string cadastroId, CancellationToken ct);

    /// <summary>
    /// Devolve até <paramref name="max"/> cadastros pendentes, travando as linhas até o fim da transação atual. Linhas já travadas
    /// por outra transação são puladas, então várias réplicas do dispatcher nunca pegam o mesmo cadastro.
    /// </summary>
    Task<IReadOnlyList<CadastroData>> ClaimPending(int max, CancellationToken ct);

    /// <summary>Muda o estado para Iniciado (a saga já foi entregue ao motor). Não entra no histórico.</summary>
    Task MarcarIniciado(string cadastroId, CancellationToken ct);

    /// <summary>
    /// Grava o estado e o acrescenta ao histórico, a menos que ele já seja o último (repetir o mesmo passo não duplica).
    /// Motivo e keycloakId nulos mantêm o que já está gravado.
    /// </summary>
    Task RegistrarPasso(string cadastroId, string estado, string? motivo, string? keycloakId, CancellationToken ct);
}
