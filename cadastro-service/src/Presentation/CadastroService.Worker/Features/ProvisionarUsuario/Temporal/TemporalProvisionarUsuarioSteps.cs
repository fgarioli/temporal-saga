using CadastroService.Core.Application.Features.ProvisionarUsuario;
using CadastroService.Core.Domain.Usuarios;
using Temporalio.Common;
using Temporalio.Exceptions;
using Temporalio.Workflows;

namespace CadastroService.Presentation.Worker.Features.ProvisionarUsuario.Temporal;

/// <summary>
/// Os passos da saga como activities do Temporal, chamadas pelo nome (Activities). Falhas técnicas são repetidas pelo Temporal
/// conforme a StepPolicy; uma activity que termina com erro do tipo Refused vira StepRefusedException, que a saga trata.
/// </summary>
public sealed class TemporalProvisionarUsuarioSteps : IProvisionarUsuarioSteps
{
    private readonly ActivityOptions options;

    public TemporalProvisionarUsuarioSteps(StepPolicy policy) => options = new()
    {
        StartToCloseTimeout = policy.Timeout,
        RetryPolicy = new RetryPolicy
        {
            InitialInterval = policy.InitialBackoff,
            BackoffCoefficient = policy.BackoffMultiplier,
            MaximumInterval = policy.MaxBackoff,
        },
    };

    public Task<string> CriarNoKeycloak(UsuarioData data) => Execute<string>(Activities.CriarNoKeycloak, data.Username, data.Email, data.Nome);

    public Task HabilitarUsuario(string usuarioId) => Execute(Activities.HabilitarUsuario, usuarioId);

    public Task LiberarAcesso(string keycloakId) => Execute(Activities.LiberarAcesso, keycloakId);

    public Task RejeitarUsuario(string usuarioId, string motivo) => Execute(Activities.RejeitarUsuario, usuarioId, motivo);

    public Task EnviarBoasVindas(UsuarioData data) => Execute(Activities.EnviarBoasVindas, data.UsuarioId, data.Email, data.Nome);

    public Task RegistrarPasso(string usuarioId, string etapa, string? motivo = null, string? keycloakId = null) =>
        Execute(Activities.RegistrarPasso, usuarioId, etapa, motivo, keycloakId);

    private async Task Execute(string activity, params object?[] args)
    {
        try
        {
            await Workflow.ExecuteActivityAsync(activity, args, options);
        }
        catch (ActivityFailureException e) when (e.InnerException is ApplicationFailureException { ErrorType: Activities.Refused } refusal)
        {
            throw new StepRefusedException(refusal.Message);
        }
    }

    private async Task<T> Execute<T>(string activity, params object?[] args)
    {
        try
        {
            return await Workflow.ExecuteActivityAsync<T>(activity, args, options);
        }
        catch (ActivityFailureException e) when (e.InnerException is ApplicationFailureException { ErrorType: Activities.Refused } refusal)
        {
            throw new StepRefusedException(refusal.Message);
        }
    }
}
