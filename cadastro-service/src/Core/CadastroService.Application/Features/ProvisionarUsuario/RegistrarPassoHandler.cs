using CadastroService.Core.Domain.Usuarios;

namespace CadastroService.Core.Application.Features.ProvisionarUsuario;

/// <summary>
/// Grava o andamento do usuário (etapa, histórico, motivo e id no Keycloak) que a consulta da API devolve.
/// Não grava nada: quem chamou (a activity) faz o commit.
/// </summary>
public sealed class RegistrarPassoHandler(IUsuarioRepository usuarios)
{
    public async Task Handle(string usuarioId, string etapa, string? motivo, string? keycloakId, CancellationToken ct)
    {
        var usuario = await usuarios.Find(usuarioId, ct) ?? throw new InvalidOperationException($"Usuário {usuarioId} não encontrado");
        usuario.RegistrarPasso(etapa, motivo, keycloakId);
        usuarios.Update(usuario);
    }
}
