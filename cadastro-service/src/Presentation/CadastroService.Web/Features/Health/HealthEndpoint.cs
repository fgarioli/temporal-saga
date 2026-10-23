using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CadastroService.Web.Features.Health;

public static class HealthEndpoint
{
    /// <summary>Saudável quando todos os health checks registrados passam (hoje: o banco já foi migrado).</summary>
    public static void Map(this IEndpointRouteBuilder app) =>
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            // Mantém o JSON de antes: {"status":"UP"} ou {"status":"DOWN","motivo":...}; o status HTTP (200/503) é o do health check.
            ResponseWriter = (context, report) =>
            {
                var motivo = report.Entries.Values.FirstOrDefault(e => e.Status != HealthStatus.Healthy);
                object body = report.Status == HealthStatus.Healthy
                    ? new { status = "UP" }
                    : new { status = "DOWN", motivo = motivo.Description ?? motivo.Exception?.Message };
                return context.Response.WriteAsJsonAsync(body);
            }
        });
}
