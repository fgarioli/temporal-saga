using CadastroService.Core.Domain.Usuarios;

namespace CadastroService.Core.Application.Features.ConsultarUsuario;

/// <summary>Consulta um usuário. Devolve null se ele não existe.</summary>
public sealed class ConsultarUsuarioHandler(IUsuarioRepository usuarios)
{
    public Task<Usuario?> Handle(string id, CancellationToken ct) => usuarios.Find(id, ct);
}
