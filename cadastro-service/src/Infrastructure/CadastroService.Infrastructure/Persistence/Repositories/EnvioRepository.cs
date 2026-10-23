using CadastroService.Core.Domain.Envios;
using CadastroService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CadastroService.Infrastructure.Persistence.Repositories;

public sealed class EnvioRepository(CadastroDbContext db) : IEnvioRepository
{
    public Task<bool> WasSent(string cadastroId, CancellationToken ct) => db.Envios.AnyAsync(x => x.CadastroId == cadastroId, ct);

    public void Add(Envio e) => db.Envios.Add(new EnvioEntity { CadastroId = e.CadastroId, Email = e.Email, EnviadoEm = e.EnviadoEm });
}
