using CadastroService.Core.Application.Features.CriarIdentidade;
using CadastroService.Core.Application.Features.EnviarBoasVindas;
using CadastroService.Core.Application.Features.HabilitarUsuario;
using CadastroService.Core.Application.Features.LiberarAcesso;
using CadastroService.Core.Application.Features.ProvisionarUsuario;
using CadastroService.Core.Application.Features.RejeitarUsuario;
using CadastroService.Core.Domain.Common;
using CadastroService.Core.Domain.Identity;
using Temporalio.Activities;
using Temporalio.Exceptions;

namespace CadastroService.Presentation.Worker.Features.ProvisionarUsuario.Temporal;

/// <summary>
/// As activities do workflow: a porta de entrada pela qual o Temporal chama os casos de uso do Application. O nome de cada uma é o
/// nome do método sem o sufixo Async ("CriarNoKeycloak"). Cada execução usa um DbContext novo e faz o próprio commit;
/// uma recusa de negócio vira erro não repetível do tipo Refused, que o TemporalProvisionarUsuarioSteps traduz em StepRefusedException.
/// </summary>
public sealed class ProvisionarUsuarioActivities(IServiceScopeFactory scopes)
{
    [Activity]
    public async Task<string> CriarNoKeycloakAsync(string username, string email, string nome)
    {
        using var scope = scopes.CreateScope();
        var result = await scope.ServiceProvider.GetRequiredService<CriarIdentidadeHandler>()
            .Handle(new NewIdentity(username, email, nome), ActivityExecutionContext.Current.CancellationToken);
        return result.KeycloakId ?? throw Refusal(result.Motivo!);
    }

    [Activity]
    public Task HabilitarUsuarioAsync(string usuarioId) =>
        Execute((sp, ct) => sp.GetRequiredService<HabilitarUsuarioHandler>().Handle(usuarioId, ct));

    [Activity]
    public Task LiberarAcessoAsync(string keycloakId) =>
        Execute((sp, ct) => sp.GetRequiredService<LiberarAcessoHandler>().Handle(keycloakId, ct));

    [Activity]
    public Task RejeitarUsuarioAsync(string usuarioId, string motivo) =>
        Execute((sp, ct) => sp.GetRequiredService<RejeitarUsuarioHandler>().Handle(usuarioId, motivo, ct));

    [Activity]
    public Task EnviarBoasVindasAsync(string usuarioId, string email, string nome) =>
        Execute((sp, ct) => sp.GetRequiredService<EnviarBoasVindasHandler>().Handle(usuarioId, email, nome, ct));

    /// <summary>Grava o andamento do usuário (etapa, histórico, motivo e id no Keycloak) que a consulta da API devolve.</summary>
    [Activity]
    public Task RegistrarPassoAsync(string usuarioId, string etapa, string? motivo, string? keycloakId) =>
        Execute((sp, ct) => sp.GetRequiredService<RegistrarPassoHandler>().Handle(usuarioId, etapa, motivo, keycloakId, ct));

    private async Task Execute(Func<IServiceProvider, CancellationToken, Task> operation)
    {
        var ct = ActivityExecutionContext.Current.CancellationToken;
        using var scope = scopes.CreateScope();
        await operation(scope.ServiceProvider, ct);
        await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChanges(ct);
    }

    private static ApplicationFailureException Refusal(string motivo) =>
        new(motivo, errorType: Activities.Refused, nonRetryable: true);
}
