using CadastroService.Core.Application.Features.AtivarUsuario;
using CadastroService.Core.Application.Features.CriarIdentidade;
using CadastroService.Core.Application.Features.CriarUsuario;
using CadastroService.Core.Application.Features.EnviarBoasVindas;
using CadastroService.Core.Application.Features.ProcessarCadastro;
using CadastroService.Core.Application.Features.RejeitarUsuario;
using CadastroService.Core.Domain.Common;
using Temporalio.Activities;
using Temporalio.Exceptions;

namespace CadastroService.Presentation.Worker.Features.ProcessarCadastro.Temporal;

/// <summary>
/// As activities do workflow: a porta de entrada pela qual o Temporal chama os casos de uso do Application. O nome de cada uma é o
/// nome do método sem o sufixo Async ("CriarUsuario"). Cada execução usa um DbContext novo e faz o próprio commit;
/// uma recusa de negócio vira erro não repetível do tipo Refused, que o TemporalCadastroSteps traduz em StepRefusedException.
/// </summary>
public sealed class CadastroActivities(IServiceScopeFactory scopes)
{
    [Activity]
    public Task CriarUsuarioAsync(string cadastroId, string username, string email, string nome) => Execute(async (sp, ct) =>
    {
        var result = await sp.GetRequiredService<CriarUsuarioHandler>().Handle(cadastroId, username, email, nome, ct);
        if (!result.Created) throw Refusal(result.Motivo!);
    });

    [Activity]
    public async Task<string> CriarNoKeycloakAsync(string username, string email, string nome)
    {
        using var scope = scopes.CreateScope();
        var result = await scope.ServiceProvider.GetRequiredService<CriarIdentidadeHandler>()
            .Handle(new NewIdentity(username, email, nome), ActivityExecutionContext.Current.CancellationToken);
        return result.KeycloakId ?? throw Refusal(result.Motivo!);
    }

    [Activity]
    public Task AtivarUsuarioAsync(string cadastroId, string keycloakId) =>
        Execute((sp, ct) => sp.GetRequiredService<AtivarUsuarioHandler>().Handle(cadastroId, keycloakId, ct));

    [Activity]
    public Task RejeitarUsuarioAsync(string cadastroId, string motivo) =>
        Execute((sp, ct) => sp.GetRequiredService<RejeitarUsuarioHandler>().Handle(cadastroId, motivo, ct));

    [Activity]
    public Task EnviarBoasVindasAsync(string cadastroId, string email, string nome) =>
        Execute((sp, ct) => sp.GetRequiredService<EnviarBoasVindasHandler>().Handle(cadastroId, email, nome, ct));

    /// <summary>Grava o andamento do cadastro (estado, histórico, motivo e id no Keycloak) que a consulta da API devolve.</summary>
    [Activity]
    public Task RegistrarPassoAsync(string cadastroId, string estado, string? motivo, string? keycloakId) =>
        Execute((sp, ct) => sp.GetRequiredService<RegistrarPassoHandler>().Handle(cadastroId, estado, motivo, keycloakId, ct));

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
