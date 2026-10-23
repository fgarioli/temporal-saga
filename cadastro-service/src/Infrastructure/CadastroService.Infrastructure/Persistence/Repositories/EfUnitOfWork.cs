using CadastroService.Domain.Common;

namespace CadastroService.Infrastructure.Persistence.Repositories;

/// <summary>O DbContext já é um Unit of Work: um SaveChanges grava tudo numa só transação.</summary>
public sealed class EfUnitOfWork(CadastroDbContext db) : IUnitOfWork
{
    public Task SaveChanges(CancellationToken ct) => db.SaveChangesAsync(ct);

    public async Task<ITransaction> BeginTransaction(CancellationToken ct) =>
        new EfTransaction(await db.Database.BeginTransactionAsync(ct));

    private sealed class EfTransaction(Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction) : ITransaction
    {
        public Task Commit(CancellationToken ct) => transaction.CommitAsync(ct);

        public ValueTask DisposeAsync() => transaction.DisposeAsync();
    }
}
