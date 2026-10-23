namespace CadastroService.Domain.Common;

/// <summary>Uma transação aberta por <see cref="IUnitOfWork.BeginTransaction"/>. Sem Commit, o Dispose desfaz tudo.</summary>
public interface ITransaction : IAsyncDisposable
{
    Task Commit(CancellationToken ct);
}
