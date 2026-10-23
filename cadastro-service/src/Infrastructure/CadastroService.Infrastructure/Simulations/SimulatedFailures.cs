using CadastroService.Core.Domain.Simulations;
using CadastroService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CadastroService.Infrastructure.Simulations;

/// <summary>
/// Só para demonstração: as próximas N criações no Keycloak falham com um erro técnico, para mostrar que o Temporal
/// resolve falhas temporárias com novas tentativas. O contador fica no Postgres porque quem agenda (o container web)
/// e quem chama o Keycloak (o container worker) são processos diferentes. Ligado por Simulations:Enabled.
/// </summary>
public sealed class SimulatedFailures(CadastroDbContext db) : ISimulatedFailures
{
    public Task Schedule(int quantity, CancellationToken ct) => db.Database.ExecuteSqlInterpolatedAsync(
        $"INSERT INTO simulated_failures (id, remaining) VALUES (1, {quantity}) ON CONFLICT (id) DO UPDATE SET remaining = simulated_failures.remaining + {quantity}", ct);

    /// <summary>Gasta uma falha agendada, se houver. A atualização é atômica: duas tentativas não gastam a mesma.</summary>
    public async Task<bool> TryConsume(CancellationToken ct) =>
        await db.Database.ExecuteSqlRawAsync("UPDATE simulated_failures SET remaining = remaining - 1 WHERE id = 1 AND remaining > 0", ct) == 1;
}
