using CadastroService.Core.Domain.Simulations;
using CadastroService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CadastroService.Infrastructure.Simulations;

/// <summary>
/// Só para demonstração: grava a entidade <see cref="SimulatedFailures"/> no Postgres, uma linha por alvo. O contador fica no banco porque
/// quem agenda (o container web) e quem chama o Keycloak (o container worker) são processos diferentes. Ligado por Simulations:Enabled.
/// <para>
/// A regra (somar, gastar) é da entidade; aqui só se carrega e se grava. A gravação é atômica por comparar-e-trocar: o UPDATE só vale se
/// o contador ainda tem o valor que foi lido. Se outra tentativa mudou o contador no meio, o UPDATE não afeta nenhuma linha e a operação
/// recomeça com o valor novo, então duas tentativas nunca gastam a mesma falha. Não usa SaveChanges nem rastreamento: uma gravação do
/// contador não leva junto as alterações pendentes do DbContext da operação que chamou.
/// </para>
/// </summary>
public sealed class SimulatedFailuresStore(CadastroDbContext db) : ISimulatedFailures
{
    public Task Schedule(string alvo, int quantity, CancellationToken ct) => Change(alvo, failures =>
    {
        failures.Schedule(quantity);
        return true;
    }, createIfMissing: true, ct);

    /// <summary>Gasta uma falha agendada para o alvo, se houver.</summary>
    public Task<bool> TryConsume(string alvo, CancellationToken ct) => Change(alvo, failures => failures.TryConsume(), createIfMissing: false, ct);

    private async Task<bool> Change(string alvo, Func<SimulatedFailures, bool> change, bool createIfMissing, CancellationToken ct)
    {
        while (true)
        {
            var failures = await db.SimulatedFailures.AsNoTracking().FirstOrDefaultAsync(x => x.Id == alvo, ct);
            if (failures is null)
            {
                if (!createIfMissing) return false;
                // Primeira vez para este alvo: cria o registro. Se outro processo o criou antes (DO NOTHING), recomeça lendo o dele.
                var created = SimulatedFailures.Create(alvo);
                var result = change(created);
                var inserted = await db.Database.ExecuteSqlInterpolatedAsync(
                    $"INSERT INTO simulated_failures (id, remaining) VALUES ({created.Id}, {created.Remaining}) ON CONFLICT (id) DO NOTHING", ct);
                if (inserted == 1) return result;
                continue;
            }

            var before = failures.Remaining;
            var outcome = change(failures);
            if (failures.Remaining == before) return outcome;
            var after = failures.Remaining;
            var updated = await db.SimulatedFailures
                .Where(x => x.Id == failures.Id && x.Remaining == before)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Remaining, after), ct);
            if (updated == 1) return outcome;
        }
    }
}
