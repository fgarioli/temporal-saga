using Microsoft.EntityFrameworkCore;

namespace CadastroService.Infrastructure.Persistence;

public static class DatabaseMigrator
{
    /// <summary>Aplica as migrations do EF Core ao subir, tentando de novo enquanto o Postgres sobe.</summary>
    public static async Task ApplyAsync<TContext>(IServiceProvider services, ILogger logger) where TContext : DbContext
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                using var scope = services.CreateScope();
                await scope.ServiceProvider.GetRequiredService<TContext>().Database.MigrateAsync();
                return;
            }
            catch (Exception e) when (attempt < 30 && e is Npgsql.NpgsqlException or InvalidOperationException)
            {
                logger.LogWarning("Postgres indisponível (tentativa {Attempt}): {Message}", attempt, e.Message);
                await Task.Delay(1000);
            }
        }
    }
}
