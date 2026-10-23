using Microsoft.Extensions.Options;
using Temporalio.Client;

namespace CadastroService.Presentation.Worker.Features.ProcessarCadastro.Temporal;

/// <summary>Conexão com o servidor do Temporal, aberta na primeira vez que alguém precisa dela.</summary>
public sealed class TemporalConnection(IOptions<TemporalOptions> options, ILogger<TemporalConnection> logger)
{
    private readonly SemaphoreSlim semaphore = new(1, 1);
    private TemporalClient? client;

    public async Task<TemporalClient> GetClient(CancellationToken ct)
    {
        if (client is not null) return client;
        await semaphore.WaitAsync(ct);
        try
        {
            for (var attempt = 1; client is null; attempt++)
            {
                try
                {
                    var connected = await TemporalClient.ConnectAsync(new(options.Value.Address) { Namespace = options.Value.Namespace });
                    // Na primeira subida, o servidor fica no ar antes de o namespace existir: espera ele ser criado.
                    await connected.WorkflowService.DescribeNamespaceAsync(
                        new Temporalio.Api.WorkflowService.V1.DescribeNamespaceRequest { Namespace = options.Value.Namespace });
                    client = connected;
                }
                catch (Exception e) when (attempt < 60 && e is not OperationCanceledException)
                {
                    logger.LogWarning("Temporal ainda não está pronto (tentativa {Attempt}): {Message}", attempt, e.Message);
                    await Task.Delay(2000, ct);
                }
            }
            return client;
        }
        finally
        {
            semaphore.Release();
        }
    }
}
