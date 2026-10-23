using CadastroService.Core.Application.Common.Ports;
using CadastroService.Core.Application.Features.CriarIdentidade;
using CadastroService.Core.Application.Features.EnviarBoasVindas;
using CadastroService.Core.Application.Features.SimularFalhas;
using CadastroService.Core.Domain.Common;
using CadastroService.Core.Domain.Envios;
using CadastroService.Core.Domain.Usuarios;
using CadastroService.Infrastructure.Email;
using CadastroService.Infrastructure.Identity;
using CadastroService.Infrastructure.Persistence;
using CadastroService.Infrastructure.Persistence.Repositories;
using CadastroService.Infrastructure.Simulations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CadastroService.Infrastructure.IoC;

internal static class InfrastructureExtensions
{
    /// <summary>Registra todas as implementações das portas do Domain e do Application. Usado pelos dois containers, web e worker.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<CadastroDbContext>(o => o.UseNpgsql(config.GetConnectionString("Cadastro")
            ?? throw new InvalidOperationException("ConnectionStrings:Cadastro não configurada")));
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<ICadastroRepository, CadastroRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IEnvioRepository, EnvioRepository>();

        services.Configure<KeycloakOptions>(config.GetSection("Keycloak"));
        services.AddHttpClient<KeycloakClient>(c => c.Timeout = TimeSpan.FromSeconds(10));
        if (Simulations.Enabled(config))
        {
            services.AddScoped<SimulatedFailures>();
            services.AddScoped<ISimulatedFailures>(sp => sp.GetRequiredService<SimulatedFailures>());
            services.AddScoped<IIdentityProvider, KeycloakWithSimulatedFailures>();
        }
        else
        {
            services.AddScoped<IIdentityProvider>(sp => sp.GetRequiredService<KeycloakClient>());
        }

        services.Configure<SmtpOptions>(config.GetSection("Smtp"));
        services.AddSingleton<IEmailSender, SmtpEmailSender>();
        return services;
    }
}
