using CadastroService.Domain.Common;
using CadastroService.Domain.Usuarios;

namespace CadastroService.Application.Features.ExcluirUsuario;

/// <summary>
/// Exclui o usuário e grava. Os desfechos esperados (não existe, cadastro em andamento, conflito de versão)
/// voltam como <see cref="UsuarioDeletionResult"/>, não como exceção.
/// </summary>
public sealed class ExcluirUsuarioHandler(IUsuarioRepository usuarios, IUnitOfWork unitOfWork)
{
    public async Task<UsuarioDeletionResult> Handle(string id, CancellationToken ct)
    {
        if (await usuarios.Find(id, ct) is not { } usuario) return new UsuarioDeletionResult(UsuarioDeletionStatus.NotFound);
        try
        {
            usuario.GarantirQuePodeSerExcluido();
        }
        catch (DomainException e)
        {
            return new UsuarioDeletionResult(UsuarioDeletionStatus.CadastroEmAndamento, e.Message);
        }
        usuarios.Remove(usuario);
        try
        {
            await unitOfWork.SaveChanges(ct);
        }
        catch (ConcurrencyConflictException)
        {
            return new UsuarioDeletionResult(UsuarioDeletionStatus.Conflict);
        }
        return new UsuarioDeletionResult(UsuarioDeletionStatus.Deleted);
    }
}
