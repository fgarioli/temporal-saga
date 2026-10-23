using CadastroService.Core.Domain.Usuarios;

namespace CadastroService.Core.Application.Features.CriarUsuario;

/// <summary>T1. Não grava nada: quem chamou (a activity) faz o commit.</summary>
public sealed class CriarUsuarioHandler(IUsuarioRepository usuarios, ILogger<CriarUsuarioHandler> logger)
{
    /// <summary>Repetir com os mesmos dados não muda o resultado; um username já usado é recusado.</summary>
    public async Task<UsuarioCreationResult> Handle(string cadastroId, string username, string email, string nome, CancellationToken ct)
    {
        if (await usuarios.Find(cadastroId, ct) is not null) return UsuarioCreationResult.Ok;
        if (await usuarios.IsUsernameInUse(username, cadastroId, ct)) return UsuarioCreationResult.Refused($"O username {username} já está em uso");
        usuarios.Add(Usuario.Create(cadastroId, username, email, nome));
        logger.LogInformation("Usuário {Id} criado como pendente", cadastroId);
        return UsuarioCreationResult.Ok;
    }
}
