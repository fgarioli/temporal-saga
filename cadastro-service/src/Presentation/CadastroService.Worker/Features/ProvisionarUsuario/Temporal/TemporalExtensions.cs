using CadastroService.Core.Application.Features.ProvisionarUsuario;

namespace CadastroService.Presentation.Worker.Features.ProvisionarUsuario.Temporal;

public static class TemporalExtensions
{
    /// <summary>Liga o Temporal ao Worker: a conexão, o ISagaRunner que inicia os workflows e o worker que os executa.</summary>
    public static IServiceCollection AddTemporal(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<TemporalOptions>(config.GetSection("Temporal"));
        services.AddSingleton<TemporalConnection>();
        services.AddSingleton<ISagaRunner, TemporalSagaRunner>();
        services.AddSingleton<ProvisionarUsuarioActivities>();
        services.AddHostedService<ProvisionarUsuarioWorker>();
        return services;
    }
}
