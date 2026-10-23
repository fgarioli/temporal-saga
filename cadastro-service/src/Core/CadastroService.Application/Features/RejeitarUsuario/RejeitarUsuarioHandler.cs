using CadastroService.Domain.Usuarios;

namespace CadastroService.Application.Features.RejeitarUsuario;

/// <summary>C1, a compensação de T1. Não grava nada: quem chamou (a activity) faz o commit.</summary>
public sealed class RejeitarUsuarioHandler(IUsuarioRepository usuarios, ILogger<RejeitarUsuarioHandler> logger)
{
    public async Task Handle(string cadastroId, string motivo, CancellationToken ct)
    {
        var usuario = await usuarios.Find(cadastroId, ct) ?? throw new InvalidOperationException($"Usuário {cadastroId} não encontrado");
        usuario.Rejeitar(motivo);
        usuarios.Update(usuario);
        logger.LogInformation("Usuário {Id} agora está {Status}", cadastroId, usuario.Status);
    }
}
