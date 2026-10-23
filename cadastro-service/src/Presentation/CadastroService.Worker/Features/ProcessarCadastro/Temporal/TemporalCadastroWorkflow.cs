using CadastroService.Core.Domain.Cadastros;
using CadastroService.Core.Application.Features.ProcessarCadastro;
using Temporalio.Workflows;

namespace CadastroService.Presentation.Worker.Features.ProcessarCadastro.Temporal;

/// <summary>
/// O workflow do Temporal, fino: só liga a saga (CadastroSaga, no Application) aos passos que chamam activities.
/// O Temporal guarda o histórico de cada passo: se o processo cair no meio, outro worker reconstrói o estado e continua de onde parou.
/// </summary>
[Workflow(Saga.Workflow)]
public sealed class TemporalCadastroWorkflow
{
    [WorkflowRun]
    public Task<string> RunAsync(CadastroData data) => new CadastroSaga(new TemporalCadastroSteps(StepPolicy.Default)).Run(data);
}
