using CadastroService.Core.Domain.Usuarios;

namespace CadastroService.Presentation.Web.Common;

/// <summary>O usuário como a API o devolve: a identidade, se está habilitado e o andamento da saga que o provisiona.</summary>
public sealed record UsuarioResponse(string Id, string Username, string Email, string Nome, bool Habilitado, string Etapa,
    IReadOnlyList<string> Historico, string? Motivo, string? KeycloakId)
{
    public static UsuarioResponse From(Usuario u) =>
        new(u.Id, u.Username, u.Email, u.Nome, u.Habilitado, u.Etapa, u.Historico, u.Motivo, u.KeycloakId);
}
