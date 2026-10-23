using CadastroService.Domain.Usuarios;

namespace CadastroService.Application.Features.ExcluirUsuario;

/// <summary>Não grava nada: quem chamou (a API) faz o commit.</summary>
public sealed class ExcluirUsuarioHandler(IUsuarioRepository usuarios)
{
    /// <summary>Devolve false se o usuário não existe; lança DomainException se o cadastro ainda está em andamento.</summary>
    public async Task<bool> Handle(string id, CancellationToken ct)
    {
        if (await usuarios.Find(id, ct) is not { } usuario) return false;
        usuario.GarantirQuePodeSerExcluido();
        usuarios.Remove(usuario);
        return true;
    }
}
