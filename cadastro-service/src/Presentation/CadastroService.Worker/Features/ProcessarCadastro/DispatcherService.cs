using CadastroService.Application.Features.ProcessarCadastro;

namespace CadastroService.Worker.Features.ProcessarCadastro;

/// <summary>
/// Entrega os cadastros pendentes à saga: a cada segundo, roda um ciclo do CadastroDispatcher num escopo novo (um DbContext por ciclo).
/// Um erro no ciclo é registrado e o próximo ciclo tenta de novo; o serviço não cai.
/// </summary>
public sealed class DispatcherService(IServiceScopeFactory scopes, ILogger<DispatcherService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        try
        {
            do
            {
                await RunCycle(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task RunCycle(CancellationToken ct)
    {
        try
        {
            using var scope = scopes.CreateScope();
            await scope.ServiceProvider.GetRequiredService<CadastroDispatcher>().Dispatch(ct);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            logger.LogError(e, "Falha ao entregar cadastros pendentes; tenta de novo no próximo ciclo");
        }
    }
}
