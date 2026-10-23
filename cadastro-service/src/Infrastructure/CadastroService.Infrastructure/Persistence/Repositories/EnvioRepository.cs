using CadastroService.Core.Domain.Envios;
using Microsoft.EntityFrameworkCore;

namespace CadastroService.Infrastructure.Persistence.Repositories;

public sealed class EnvioRepository(CadastroDbContext db) : IEnvioRepository
{
    public Task<bool> WasSent(string usuarioId, CancellationToken ct) => db.Envios.AnyAsync(x => x.UsuarioId == usuarioId, ct);

    public void Add(Envio envio) => db.Envios.Add(envio);
}
