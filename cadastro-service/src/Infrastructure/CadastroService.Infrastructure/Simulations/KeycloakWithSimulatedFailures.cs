using CadastroService.Core.Domain.Identity;
using CadastroService.Core.Domain.Simulations;
using CadastroService.Infrastructure.Identity;

namespace CadastroService.Infrastructure.Simulations;

/// <summary>Decorator do Keycloak que obedece às falhas agendadas: keycloak-create em Create e keycloak-enable em Enable.</summary>
public sealed class KeycloakWithSimulatedFailures(KeycloakClient real, SimulatedFailuresStore failures) : IIdentityProvider
{
    public async Task<string> Create(NewIdentity newIdentity, CancellationToken ct) =>
        await failures.TryConsume(SimulatedFailures.KeycloakCreate, ct)
            ? throw new HttpRequestException("Falha simulada: Keycloak indisponível")
            : await real.Create(newIdentity, ct);

    public async Task Enable(string keycloakId, CancellationToken ct)
    {
        if (await failures.TryConsume(SimulatedFailures.KeycloakEnable, ct)) throw new HttpRequestException("Falha simulada: Keycloak indisponível");
        await real.Enable(keycloakId, ct);
    }

    public Task<IdentityProviderUser?> FindByUsername(string username, CancellationToken ct) => real.FindByUsername(username, ct);
}
