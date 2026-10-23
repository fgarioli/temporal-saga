using CadastroService.Core.Domain.Cadastros;
using CadastroService.Core.Application.Features.ProcessarCadastro;
using Temporalio.Api.Enums.V1;
using Temporalio.Client;
using Temporalio.Exceptions;

namespace CadastroService.Presentation.Worker.Features.ProcessarCadastro.Temporal;

/// <summary>
/// Inicia o workflow no Temporal. O id do workflow é o id do cadastro, então o mesmo cadastro nunca roda duas vezes:
/// pedir de novo (por exemplo, quando a transação do dispatcher caiu depois de o workflow começar) é sucesso, não erro.
/// </summary>
public sealed class TemporalSagaRunner(TemporalConnection temporal) : ISagaRunner
{
    public async Task Start(CadastroData data, CancellationToken ct)
    {
        var client = await temporal.GetClient(ct);
        var options = new WorkflowOptions(id: data.CadastroId, taskQueue: Saga.TaskQueue)
        {
            // Mesmo com o workflow já terminado, o id não é reaproveitado: o cadastro não roda de novo.
            IdReusePolicy = WorkflowIdReusePolicy.RejectDuplicate,
        };
        try
        {
            await client.StartWorkflowAsync(Saga.Workflow, new object?[] { data }, options);
        }
        catch (WorkflowAlreadyStartedException)
        {
        }
    }
}
