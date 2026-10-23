namespace CadastroService.Domain.Usuarios;

public interface IUsuarioRepository
{
    Task<Usuario?> Find(string id, CancellationToken ct);
    Task<bool> IsUsernameInUse(string username, string exceptId, CancellationToken ct);
    void Add(Usuario usuario);
    void Update(Usuario usuario);
    void Remove(Usuario usuario);
}
