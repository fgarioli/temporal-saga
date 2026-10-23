namespace CadastroService.Core.Domain.Cadastros;

/// <summary>
/// Porta de saída: onde o cadastro (o pedido e o andamento da saga) fica gravado. Nenhum método grava sozinho;
/// quem chamou faz o commit pelo IUnitOfWork. As alterações feitas na entidade devolvida por Get ou ClaimPending são gravadas no commit.
/// </summary>
public interface ICadastroRepository
{
    void Add(Cadastro cadastro);

    /// <summary>Devolve o cadastro para ser consultado ou alterado, ou null se ele não existe.</summary>
    Task<Cadastro?> Get(string cadastroId, CancellationToken ct);

    /// <summary>
    /// Devolve até <paramref name="max"/> cadastros pendentes, travando as linhas até o fim da transação atual. Linhas já travadas
    /// por outra transação são puladas, então várias réplicas do dispatcher nunca pegam o mesmo cadastro.
    /// </summary>
    Task<IReadOnlyList<Cadastro>> ClaimPending(int max, CancellationToken ct);
}
