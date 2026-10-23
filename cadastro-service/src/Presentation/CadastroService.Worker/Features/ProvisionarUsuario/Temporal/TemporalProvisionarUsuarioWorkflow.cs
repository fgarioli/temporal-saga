using CadastroService.Core.Application.Features.ProvisionarUsuario;
using CadastroService.Core.Domain.Usuarios;
using Temporalio.Workflows;

namespace CadastroService.Presentation.Worker.Features.ProvisionarUsuario.Temporal;

/// <summary>
/// O workflow do Temporal, fino: só liga a saga (ProvisionarUsuarioSaga, no Application) aos passos que chamam activities.
/// O Temporal guarda o histórico de cada passo: se o processo cair no meio, outro worker reconstrói o estado e continua de onde parou.
/// </summary>
[Workflow(Saga.Workflow)]
public sealed class TemporalProvisionarUsuarioWorkflow
{
    [WorkflowRun]
    public Task<string> RunAsync(UsuarioData data) => new ProvisionarUsuarioSaga(new TemporalProvisionarUsuarioSteps(StepPolicy.Default)).Run(data);
}
