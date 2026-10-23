using CadastroService.Application.Features.ProcessarCadastro;

namespace CadastroService.Worker.Features.ProcessarCadastro.Temporal;

public static class TemporalExtensions
{
    /// <summary>Liga o Temporal ao Worker: a conexão, o ISagaRunner que inicia os workflows e o worker que os executa.</summary>
    public static IServiceCollection AddTemporal(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<TemporalOptions>(config.GetSection("Temporal"));
        services.AddSingleton<TemporalConnection>();
        services.AddSingleton<ISagaRunner, TemporalSagaRunner>();
        services.AddSingleton<CadastroActivities>();
        services.AddHostedService<CadastroWorker>();
        return services;
    }
}
