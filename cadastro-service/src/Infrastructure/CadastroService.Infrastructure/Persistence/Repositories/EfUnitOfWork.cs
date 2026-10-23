using CadastroService.Core.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace CadastroService.Infrastructure.Persistence.Repositories;

/// <summary>O DbContext já é um Unit of Work: um SaveChanges grava tudo numa só transação.</summary>
public sealed class EfUnitOfWork(CadastroDbContext db) : IUnitOfWork
{
    public async Task SaveChanges(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException e)
        {
            throw new ConcurrencyConflictException("O registro mudou depois de ser lido", e);
        }
    }

    public async Task<ITransaction> BeginTransaction(CancellationToken ct) =>
        new EfTransaction(await db.Database.BeginTransactionAsync(ct));

    private sealed class EfTransaction(Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction) : ITransaction
    {
        public Task Commit(CancellationToken ct) => transaction.CommitAsync(ct);

        public ValueTask DisposeAsync() => transaction.DisposeAsync();
    }
}
