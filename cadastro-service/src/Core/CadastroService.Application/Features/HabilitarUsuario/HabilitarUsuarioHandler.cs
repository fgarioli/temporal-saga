using CadastroService.Core.Domain.Usuarios;

namespace CadastroService.Core.Application.Features.HabilitarUsuario;

/// <summary>T3, repetível. Não grava nada: quem chamou (a activity) faz o commit.</summary>
public sealed class HabilitarUsuarioHandler(IUsuarioRepository usuarios, ILogger<HabilitarUsuarioHandler> logger)
{
    public async Task Handle(string usuarioId, CancellationToken ct)
    {
        var usuario = await usuarios.Find(usuarioId, ct) ?? throw new InvalidOperationException($"Usuário {usuarioId} não encontrado");
        usuario.Habilitar();
        usuarios.Update(usuario);
        logger.LogInformation("Usuário {Id} habilitado", usuarioId);
    }
}
