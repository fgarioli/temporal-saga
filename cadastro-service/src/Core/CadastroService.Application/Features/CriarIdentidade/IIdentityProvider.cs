namespace CadastroService.Core.Application.Features.CriarIdentidade;

/// <summary>Porta de saída para o Keycloak. Create lança IdentityAlreadyExistsException em caso de conflito.</summary>
public interface IIdentityProvider
{
    Task<string> Create(NewIdentity newIdentity, CancellationToken ct);
    Task<IdentityProviderUser?> FindByUsername(string username, CancellationToken ct);
}
