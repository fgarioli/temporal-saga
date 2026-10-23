using CadastroService.Domain.Usuarios;

namespace CadastroService.Application.Features.AtivarUsuario;

/// <summary>T3, repetível. Não grava nada: quem chamou (a activity) faz o commit.</summary>
public sealed class AtivarUsuarioHandler(IUsuarioRepository usuarios, ILogger<AtivarUsuarioHandler> logger)
{
    public async Task Handle(string cadastroId, string keycloakId, CancellationToken ct)
    {
        var usuario = await usuarios.Find(cadastroId, ct) ?? throw new InvalidOperationException($"Usuário {cadastroId} não encontrado");
        usuario.Ativar(keycloakId);
        usuarios.Update(usuario);
        logger.LogInformation("Usuário {Id} agora está {Status}", cadastroId, usuario.Status);
    }
}
