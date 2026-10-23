using CadastroService.Core.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace CadastroService.Infrastructure.Persistence.Repositories;

public sealed class UsuarioRepository(CadastroDbContext db) : IUsuarioRepository
{
    public async Task<Usuario?> Find(string id, CancellationToken ct) => await db.Usuarios.FindAsync([id], ct);

    public Task<bool> IsUsernameInUse(string username, string exceptId, CancellationToken ct) =>
        db.Usuarios.AnyAsync(u => u.Username == username && u.Id != exceptId && u.Etapa != Steps.Cancelado, ct);

    public void Add(Usuario usuario) => db.Usuarios.Add(usuario);

    /// <summary>
    /// O usuário carregado já é rastreado pelo DbContext, que grava as mudanças no commit e confere a versão (xmin).
    /// Só confirma que ele foi carregado antes de ser alterado.
    /// </summary>
    public void Update(Usuario usuario) => EnsureTracked(usuario);

    public void Remove(Usuario usuario) => db.Usuarios.Remove(EnsureTracked(usuario));

    /// <summary>
    /// FOR UPDATE SKIP LOCKED: trava as linhas escolhidas e pula as que outra transação já travou. O xmin não entra no SELECT *,
    /// então é pedido à parte: o EF o usa como versão da linha.
    /// </summary>
    public async Task<IReadOnlyList<Usuario>> ClaimPending(int max, CancellationToken ct) =>
        await db.Usuarios
            .FromSqlInterpolated($"SELECT *, xmin FROM usuarios WHERE etapa = {Steps.Pendente} ORDER BY criado_em LIMIT {max} FOR UPDATE SKIP LOCKED")
            .ToListAsync(ct);

    private Usuario EnsureTracked(Usuario usuario) => db.Entry(usuario).State != EntityState.Detached
        ? usuario
        : throw new InvalidOperationException($"O usuário {usuario.Id} precisa ser carregado antes de ser alterado");
}
