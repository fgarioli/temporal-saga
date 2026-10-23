using CadastroService.Core.Application.Features.AtivarUsuario;
using CadastroService.Core.Application.Features.Cadastrar;
using CadastroService.Core.Application.Features.ConsultarCadastro;
using CadastroService.Core.Application.Features.ConsultarUsuario;
using CadastroService.Core.Application.Features.CriarIdentidade;
using CadastroService.Core.Domain.Identity;
using CadastroService.Core.Application.Features.EnviarBoasVindas;
using CadastroService.Core.Domain.Notifications;
using CadastroService.Core.Application.Features.ExcluirUsuario;
using CadastroService.Core.Application.Features.ProcessarCadastro;
using CadastroService.Core.Application.Features.RejeitarUsuario;
using CadastroService.Core.Application.Features.SimularFalhas;
using CadastroService.Core.Domain.Simulations;
using CadastroService.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CadastroService.Infrastructure.IoC;

public static class DependencyInjection
{
    /// <summary>Container "web": a infraestrutura e os handlers da API (cadastrar, consultar, usuários). Não conhece a saga.</summary>
    public static IServiceCollection AddCadastroWeb(this IServiceCollection services, IConfiguration config)
    {
        services.AddInfrastructure(config);
        services.AddHealthChecks().AddCheck<PendingMigrationsHealthCheck>("database");
        services.AddCadastrar();
        services.AddConsultarCadastro();
        services.AddConsultarUsuario();
        services.AddExcluirUsuario();
        if (Simulations.Enabled(config)) services.AddSimularFalhas();
        return services;
    }

    /// <summary>
    /// Container "worker": a infraestrutura, o dispatcher e os handlers que os passos da saga chamam. O ISagaRunner (o motor)
    /// é registrado pelo próprio Worker.
    /// </summary>
    public static IServiceCollection AddCadastroWorker(this IServiceCollection services, IConfiguration config)
    {
        services.AddInfrastructure(config);
        services.AddProcessarCadastro();
        services.AddCriarIdentidade();
        services.AddAtivarUsuario();
        services.AddRejeitarUsuario();
        services.AddEnviarBoasVindas();
        return services;
    }

    private static void AddCadastrar(this IServiceCollection services) => services.AddScoped<CadastrarHandler>();

    private static void AddConsultarCadastro(this IServiceCollection services) => services.AddScoped<ConsultarCadastroHandler>();

    private static void AddConsultarUsuario(this IServiceCollection services) => services.AddScoped<ConsultarUsuarioHandler>();

    private static void AddExcluirUsuario(this IServiceCollection services) => services.AddScoped<ExcluirUsuarioHandler>();

    private static void AddSimularFalhas(this IServiceCollection services) => services.AddScoped<SimularFalhasHandler>();

    private static void AddProcessarCadastro(this IServiceCollection services)
    {
        services.AddScoped<CadastroDispatcher>();
        services.AddScoped<RegistrarPassoHandler>();
    }

    private static void AddCriarIdentidade(this IServiceCollection services) => services.AddScoped<CriarIdentidadeHandler>();

    private static void AddAtivarUsuario(this IServiceCollection services) => services.AddScoped<AtivarUsuarioHandler>();

    private static void AddRejeitarUsuario(this IServiceCollection services) => services.AddScoped<RejeitarUsuarioHandler>();

    private static void AddEnviarBoasVindas(this IServiceCollection services) => services.AddScoped<EnviarBoasVindasHandler>();
}
