using CadastroService.Core.Domain.Common;
using CadastroService.Core.Domain.Usuarios;

namespace CadastroService.Core.Application.Features.CriarUsuario;

/// <summary>
/// T1, síncrono: grava o usuário Pendente e desabilitado. A API não executa a saga: o dispatcher entrega o usuário ao motor,
/// que começa pelo Keycloak.
/// </summary>
public sealed class CriarUsuarioHandler(IUsuarioRepository usuarios, IUnitOfWork unitOfWork, ILogger<CriarUsuarioHandler> logger)
{
    public async Task<UsuarioCreationResult> Handle(CriarUsuarioCommand command, CancellationToken ct)
    {
        var id = Guid.NewGuid().ToString();
        if (await usuarios.IsUsernameInUse(command.Username, id, ct)) return UsernameEmUso(command.Username);

        var usuario = Usuario.Create(id, command.Username, command.Email, command.Nome, DateTimeOffset.UtcNow);
        usuarios.Add(usuario);
        try
        {
            await unitOfWork.SaveChanges(ct);
        }
        catch (UniqueConstraintViolationException)
        {
            // Outro usuário com o mesmo username gravou entre a checagem e o commit: o índice único decide.
            return UsernameEmUso(command.Username);
        }
        logger.LogInformation("Usuário {Id} criado para {Username}", id, command.Username);
        return new UsuarioCreationResult.Created(usuario);
    }

    private static UsuarioCreationResult.UsernameEmUso UsernameEmUso(string username) => new($"O username {username} já está em uso");
}
