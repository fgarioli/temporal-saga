using Temporalio.Worker;

namespace CadastroService.Presentation.Worker.Features.ProvisionarUsuario.Temporal;

/// <summary>
/// O worker do Temporal: fica buscando tarefas na task queue "provisionamento" e executa o workflow e as activities.
/// Dá para subir várias réplicas deste container; o Temporal distribui as tarefas entre elas.
/// </summary>
public sealed class ProvisionarUsuarioWorker(TemporalConnection temporal, ProvisionarUsuarioActivities activities, ILogger<ProvisionarUsuarioWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var client = await temporal.GetClient(stoppingToken);
        using var worker = new TemporalWorker(client, new TemporalWorkerOptions(Saga.TaskQueue)
            .AddWorkflow<TemporalProvisionarUsuarioWorkflow>()
            .AddAllActivities(activities));
        logger.LogInformation("Worker da task queue {TaskQueue} no ar", Saga.TaskQueue);
        await worker.ExecuteAsync(stoppingToken);
    }
}
