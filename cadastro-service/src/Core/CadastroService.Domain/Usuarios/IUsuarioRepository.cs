namespace CadastroService.Core.Domain.Usuarios;

/// <summary>
/// Porta de saída: onde o usuário (a identidade da aplicação e o andamento da saga) fica gravado. Nenhum método grava sozinho;
/// quem chamou faz o commit pelo IUnitOfWork. As alterações feitas na entidade devolvida por Find ou ClaimPending são gravadas no commit.
/// </summary>
public interface IUsuarioRepository
{
    Task<Usuario?> Find(string id, CancellationToken ct);
    Task<bool> IsUsernameInUse(string username, string exceptId, CancellationToken ct);
    void Add(Usuario usuario);
    void Update(Usuario usuario);
    void Remove(Usuario usuario);

    /// <summary>
    /// Devolve até <paramref name="max"/> usuários na etapa Pendente, travando as linhas até o fim da transação atual. Linhas já travadas
    /// por outra transação são puladas, então várias réplicas do dispatcher nunca pegam o mesmo usuário.
    /// </summary>
    Task<IReadOnlyList<Usuario>> ClaimPending(int max, CancellationToken ct);
}
