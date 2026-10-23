using CadastroService.Core.Application.Features.CriarIdentidade;
using CadastroService.Infrastructure.Identity;

namespace CadastroService.Infrastructure.Simulations;

/// <summary>Decorator do Keycloak que obedece às falhas agendadas.</summary>
public sealed class KeycloakWithSimulatedFailures(KeycloakClient real, SimulatedFailures failures) : IIdentityProvider
{
    public async Task<string> Create(NewIdentity newIdentity, CancellationToken ct) =>
        await failures.TryConsume(ct) ? throw new HttpRequestException("Falha simulada: Keycloak indisponível") : await real.Create(newIdentity, ct);

    public Task<IdentityProviderUser?> FindByUsername(string username, CancellationToken ct) => real.FindByUsername(username, ct);
}
