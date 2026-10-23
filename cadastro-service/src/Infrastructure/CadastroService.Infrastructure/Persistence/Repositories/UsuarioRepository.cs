using CadastroService.Core.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace CadastroService.Infrastructure.Persistence.Repositories;

public sealed class UsuarioRepository(CadastroDbContext db) : IUsuarioRepository
{
    public async Task<Usuario?> Find(string id, CancellationToken ct) => await db.Usuarios.FindAsync([id], ct);

    public Task<bool> IsUsernameInUse(string username, string exceptId, CancellationToken ct) =>
        db.Usuarios.AnyAsync(u => u.Username == username && u.Id != exceptId && u.Status != StatusUsuario.Rejeitado, ct);

    public void Add(Usuario usuario) => db.Usuarios.Add(usuario);

    /// <summary>
    /// O usuário carregado já é rastreado pelo DbContext, que grava as mudanças no commit e confere a versão (xmin).
    /// Só confirma que ele foi carregado antes de ser alterado.
    /// </summary>
    public void Update(Usuario usuario) => EnsureTracked(usuario);

    public void Remove(Usuario usuario) => db.Usuarios.Remove(EnsureTracked(usuario));

    private Usuario EnsureTracked(Usuario usuario) => db.Entry(usuario).State != EntityState.Detached
        ? usuario
        : throw new InvalidOperationException($"O usuário {usuario.Id} precisa ser carregado antes de ser alterado");
}
