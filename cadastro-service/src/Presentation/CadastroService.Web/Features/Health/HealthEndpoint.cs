using CadastroService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CadastroService.Web.Features.Health;

public static class HealthEndpoint
{
    /// <summary>Saudável quando o banco já foi migrado (quem migra é o container worker).</summary>
    public static void Map(this IEndpointRouteBuilder app) =>
        app.MapGet("/health", async (CadastroDbContext db, CancellationToken ct) =>
        {
            var pending = await db.Database.GetPendingMigrationsAsync(ct);
            return pending.Any()
                ? Results.Json(new { status = "DOWN", motivo = "aguardando as migrations" }, statusCode: 503)
                : Results.Ok(new { status = "UP" });
        });
}
