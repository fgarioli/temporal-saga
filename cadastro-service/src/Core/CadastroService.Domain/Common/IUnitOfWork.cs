namespace CadastroService.Domain.Common;

/// <summary>Grava tudo o que foi alterado numa única transação. Um conflito de versão lança <see cref="ConcurrencyConflictException"/>.</summary>
public interface IUnitOfWork
{
    Task SaveChanges(CancellationToken ct);

    /// <summary>
    /// Abre uma transação explícita, para quem precisa manter travas de linha entre a leitura e a gravação.
    /// Sair do escopo sem chamar <see cref="ITransaction.Commit"/> desfaz tudo (rollback).
    /// </summary>
    Task<ITransaction> BeginTransaction(CancellationToken ct);
}
