namespace CadastroService.Application.Features.CriarUsuario;

/// <summary>O resultado de criar um usuário: criado, ou recusado com o motivo.</summary>
public sealed record UsuarioCreationResult(bool Created, string? Motivo)
{
    public static readonly UsuarioCreationResult Ok = new(true, null);
    public static UsuarioCreationResult Refused(string motivo) => new(false, motivo);
}
