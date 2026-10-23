using CadastroService.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace CadastroService.IoC;

public static class MigrationsExtensions
{
    /// <summary>Aplica as migrations do banco do cadastro ao subir, tentando de novo enquanto o Postgres sobe.</summary>
    public static Task ApplyMigrationsAsync(this IServiceProvider services, ILogger logger) =>
        DatabaseMigrator.ApplyAsync<CadastroDbContext>(services, logger);
}
