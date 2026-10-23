using CadastroService.Core.Domain.Usuarios;

namespace CadastroService.Core.Application.Features.CriarUsuario;

/// <summary>O desfecho de criar um usuário: criado (Pendente e desabilitado) ou recusado porque o username já está em uso.</summary>
public abstract record UsuarioCreationResult
{
    public sealed record Created(Usuario Usuario) : UsuarioCreationResult;

    public sealed record UsernameEmUso(string Mensagem) : UsuarioCreationResult;
}
