using CadastroService.Core.Domain.Identity;

namespace CadastroService.Core.Application.Features.CriarIdentidade;

/// <summary>
/// T2, o pivô: cria o usuário no Keycloak. Falhas técnicas (rede, Keycloak fora do ar) sobem como exceção, e o
/// Temporal tenta de novo; um username ou e-mail que já existe é uma recusa definitiva, que dispara a compensação.
/// </summary>
public sealed class CriarIdentidadeHandler(IIdentityProvider provider, ILogger<CriarIdentidadeHandler> logger)
{
    public async Task<IdentityCreationResult> Handle(NewIdentity newIdentity, CancellationToken ct)
    {
        try
        {
            var id = await provider.Create(newIdentity, ct);
            logger.LogInformation("Usuário {Username} criado no Keycloak ({KeycloakId})", newIdentity.Username, id);
            return new IdentityCreationResult(id, null);
        }
        catch (IdentityAlreadyExistsException refusal)
        {
            // Idempotência: se uma tentativa anterior deste mesmo cadastro já criou o usuário e caiu antes de
            // registrar o resultado, o Keycloak responde "já existe". Mesmo username e mesmo e-mail: é o nosso.
            var existing = await provider.FindByUsername(newIdentity.Username, ct);
            if (existing is not null && string.Equals(existing.Email, newIdentity.Email, StringComparison.OrdinalIgnoreCase))
                return new IdentityCreationResult(existing.Id, null);
            logger.LogInformation("Keycloak recusou o cadastro de {Username}", newIdentity.Username);
            return new IdentityCreationResult(null, refusal.Message);
        }
    }
}
