namespace CadastroService.Core.Domain.Envios;

public interface IEnvioRepository
{
    Task<bool> WasSent(string usuarioId, CancellationToken ct);
    void Add(Envio envio);
}
