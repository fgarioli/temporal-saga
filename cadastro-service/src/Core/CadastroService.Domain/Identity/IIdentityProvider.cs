namespace CadastroService.Core.Domain.Identity;

/// <summary>Porta de saída para o Keycloak. Create lança IdentityAlreadyExistsException em caso de conflito.</summary>
public interface IIdentityProvider
{
    /// <summary>Cria a identidade desabilitada: o usuário ainda não consegue fazer login. Devolve o id dela no provedor.</summary>
    Task<string> Create(NewIdentity newIdentity, CancellationToken ct);

    /// <summary>Habilita a identidade, liberando o login. Repetível: habilitar uma identidade já habilitada não muda nada.</summary>
    Task Enable(string keycloakId, CancellationToken ct);

    Task<IdentityProviderUser?> FindByUsername(string username, CancellationToken ct);
}
