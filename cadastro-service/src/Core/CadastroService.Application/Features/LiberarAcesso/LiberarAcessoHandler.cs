using CadastroService.Core.Domain.Identity;

namespace CadastroService.Core.Application.Features.LiberarAcesso;

/// <summary>T4, repetível: habilita a identidade no Keycloak, e só então o usuário consegue fazer login. Falhas técnicas sobem como exceção e o Temporal tenta de novo.</summary>
public sealed class LiberarAcessoHandler(IIdentityProvider provider, ILogger<LiberarAcessoHandler> logger)
{
    public async Task Handle(string keycloakId, CancellationToken ct)
    {
        await provider.Enable(keycloakId, ct);
        logger.LogInformation("Acesso liberado no Keycloak ({KeycloakId})", keycloakId);
    }
}
