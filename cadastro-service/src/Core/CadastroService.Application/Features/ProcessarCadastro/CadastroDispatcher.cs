using CadastroService.Core.Domain.Cadastros;
using CadastroService.Core.Domain.Common;

namespace CadastroService.Core.Application.Features.ProcessarCadastro;

/// <summary>
/// Entrega os cadastros pendentes ao motor da saga. Cada ciclo roda numa transação: pega os pendentes (com as linhas travadas),
/// inicia a saga de cada um e os marca como Iniciado. Se iniciar a saga falhar, a transação é desfeita e o cadastro continua
/// pendente para o próximo ciclo. Como iniciar a saga é idempotente, repetir um cadastro cuja transação caiu é seguro.
/// </summary>
public sealed class CadastroDispatcher(ICadastroRepository cadastros, IUnitOfWork unitOfWork, ISagaRunner runner, ILogger<CadastroDispatcher> logger)
{
    public const int BatchSize = 10;

    /// <summary>Devolve quantos cadastros entregou.</summary>
    public async Task<int> Dispatch(CancellationToken ct)
    {
        await using var transaction = await unitOfWork.BeginTransaction(ct);
        var pending = await cadastros.ClaimPending(BatchSize, ct);
        foreach (var data in pending)
        {
            await runner.Start(data, ct);
            await cadastros.MarcarIniciado(data.CadastroId, ct);
            logger.LogInformation("Cadastro {Id} entregue à saga", data.CadastroId);
        }
        await unitOfWork.SaveChanges(ct);
        await transaction.Commit(ct);
        return pending.Count;
    }
}
