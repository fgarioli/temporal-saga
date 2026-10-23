using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CadastroService.Infrastructure.Persistence;

/// <summary>Saudável quando o banco já foi migrado (quem migra é o container worker).</summary>
public sealed class PendingMigrationsHealthCheck(CadastroDbContext db) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var pending = await db.Database.GetPendingMigrationsAsync(cancellationToken);
        return pending.Any()
            ? HealthCheckResult.Unhealthy("aguardando as migrations")
            : HealthCheckResult.Healthy();
    }
}
