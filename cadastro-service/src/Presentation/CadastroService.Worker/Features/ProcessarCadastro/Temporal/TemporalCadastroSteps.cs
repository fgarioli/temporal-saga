using CadastroService.Core.Domain.Cadastros;
using CadastroService.Core.Application.Features.ProcessarCadastro;
using Temporalio.Common;
using Temporalio.Exceptions;
using Temporalio.Workflows;

namespace CadastroService.Presentation.Worker.Features.ProcessarCadastro.Temporal;

/// <summary>
/// Os passos da saga como activities do Temporal, chamadas pelo nome (Activities). Falhas técnicas são repetidas pelo Temporal
/// conforme a StepPolicy; uma activity que termina com erro do tipo Refused vira StepRefusedException, que a saga trata.
/// </summary>
public sealed class TemporalCadastroSteps : ICadastroSagaSteps
{
    private readonly ActivityOptions options;

    public TemporalCadastroSteps(StepPolicy policy) => options = new()
    {
        StartToCloseTimeout = policy.Timeout,
        RetryPolicy = new RetryPolicy
        {
            InitialInterval = policy.InitialBackoff,
            BackoffCoefficient = policy.BackoffMultiplier,
            MaximumInterval = policy.MaxBackoff,
        },
    };

    public Task CriarUsuario(CadastroData data) => Execute(Activities.CriarUsuario, data.CadastroId, data.Username, data.Email, data.Nome);

    public Task<string> CriarNoKeycloak(CadastroData data) => Execute<string>(Activities.CriarNoKeycloak, data.Username, data.Email, data.Nome);

    public Task AtivarUsuario(string cadastroId, string keycloakId) => Execute(Activities.AtivarUsuario, cadastroId, keycloakId);

    public Task RejeitarUsuario(string cadastroId, string motivo) => Execute(Activities.RejeitarUsuario, cadastroId, motivo);

    public Task EnviarBoasVindas(CadastroData data) => Execute(Activities.EnviarBoasVindas, data.CadastroId, data.Email, data.Nome);

    public Task RegistrarPasso(string cadastroId, string estado, string? motivo = null, string? keycloakId = null) =>
        Execute(Activities.RegistrarPasso, cadastroId, estado, motivo, keycloakId);

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
