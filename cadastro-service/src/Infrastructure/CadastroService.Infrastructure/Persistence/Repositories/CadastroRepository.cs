using CadastroService.Core.Domain.Cadastros;
using Microsoft.EntityFrameworkCore;

namespace CadastroService.Infrastructure.Persistence.Repositories;

public sealed class CadastroRepository(CadastroDbContext db) : ICadastroRepository
{
    public void Add(Cadastro cadastro) => db.Cadastros.Add(cadastro);

    /// <summary>O EF reaproveita a entidade que o ClaimPending já trouxe no mesmo DbContext.</summary>
    public async Task<Cadastro?> Get(string cadastroId, CancellationToken ct) => await db.Cadastros.FindAsync([cadastroId], ct);

    /// <summary>FOR UPDATE SKIP LOCKED: trava as linhas escolhidas e pula as que outra transação já travou.</summary>
    public async Task<IReadOnlyList<Cadastro>> ClaimPending(int max, CancellationToken ct) =>
        await db.Cadastros
            .FromSqlInterpolated($"SELECT * FROM cadastros WHERE estado = {Steps.Pendente} ORDER BY criado_em LIMIT {max} FOR UPDATE SKIP LOCKED")
            .ToListAsync(ct);
}
