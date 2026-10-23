using CadastroService.Core.Domain.Common;
using CadastroService.Core.Domain.Usuarios;

namespace CadastroService.Core.Application.Features.ProvisionarUsuario;

/// <summary>
/// Entrega os usuários pendentes ao motor da saga. Cada ciclo roda numa transação: pega os pendentes (com as linhas travadas),
/// inicia a saga de cada um e os marca como Iniciado. Se iniciar a saga falhar, a transação é desfeita e o usuário continua
/// pendente para o próximo ciclo. Como iniciar a saga é idempotente, repetir um usuário cuja transação caiu é seguro.
/// </summary>
public sealed class ProvisionamentoDispatcher(IUsuarioRepository usuarios, IUnitOfWork unitOfWork, ISagaRunner runner, ILogger<ProvisionamentoDispatcher> logger)
{
    public const int BatchSize = 10;

    /// <summary>Devolve quantos usuários entregou.</summary>
    public async Task<int> Dispatch(CancellationToken ct)
    {
        await using var transaction = await unitOfWork.BeginTransaction(ct);
        var pending = await usuarios.ClaimPending(BatchSize, ct);
        foreach (var usuario in pending)
        {
            await runner.Start(usuario.ToData(), ct);
            usuario.MarcarIniciado();
            logger.LogInformation("Usuário {Id} entregue à saga", usuario.Id);
        }
        await unitOfWork.SaveChanges(ct);
        await transaction.Commit(ct);
        return pending.Count;
    }
}
