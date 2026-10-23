using CadastroService.Domain.Usuarios;
using CadastroService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CadastroService.Infrastructure.Persistence.Repositories;

public sealed class UsuarioRepository(CadastroDbContext db) : IUsuarioRepository
{
    public async Task<Usuario?> Find(string id, CancellationToken ct)
    {
        var e = await db.Usuarios.FindAsync([id], ct);
        return e is null ? null : Usuario.Restore(e.Id, e.Username, e.Email, e.Nome, Enum.Parse<StatusUsuario>(e.Status), e.KeycloakId, e.Motivo);
    }

    public Task<bool> IsUsernameInUse(string username, string exceptId, CancellationToken ct) =>
        db.Usuarios.AnyAsync(u => u.Username == username && u.Id != exceptId && u.Status != nameof(StatusUsuario.Rejeitado), ct);

    public void Add(Usuario u) =>
        db.Usuarios.Add(CopyTo(u, new UsuarioEntity { Id = u.Id, Username = u.Username, Email = u.Email, Nome = u.Nome }));

    /// <summary>Atualiza a entidade carregada no mesmo DbContext: o EF confere a versão (xmin) ao gravar.</summary>
    public void Update(Usuario u) => CopyTo(u, GetTracked(u.Id));

    public void Remove(Usuario u) => db.Usuarios.Remove(GetTracked(u.Id));

    private UsuarioEntity GetTracked(string id) => db.Usuarios.Local.FirstOrDefault(x => x.Id == id)
        ?? throw new InvalidOperationException($"O usuário {id} precisa ser carregado antes de ser alterado");

    private static UsuarioEntity CopyTo(Usuario u, UsuarioEntity e)
    {
        e.Status = u.Status.ToString();
        e.KeycloakId = u.KeycloakId;
        e.Motivo = u.Motivo;
        return e;
    }
}
