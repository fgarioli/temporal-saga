using CadastroService.Core.Domain.Identity;

namespace CadastroService.Core.Application.Features.CriarIdentidade;

/// <summary>O resultado de criar a identidade: o id no Keycloak, ou a recusa com o motivo.</summary>
public sealed record IdentityCreationResult(string? KeycloakId, string? Motivo)
{
    public bool Refused => KeycloakId is null;
}
