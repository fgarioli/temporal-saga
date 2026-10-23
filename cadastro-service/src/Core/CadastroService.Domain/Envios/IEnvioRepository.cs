namespace CadastroService.Domain.Envios;

public interface IEnvioRepository
{
    Task<bool> WasSent(string cadastroId, CancellationToken ct);
    void Add(Envio envio);
}
