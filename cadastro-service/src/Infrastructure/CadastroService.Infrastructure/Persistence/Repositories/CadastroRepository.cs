using System.Text.Json;
using CadastroService.Application.Common.Cadastros;
using CadastroService.Application.Common.Ports;
using CadastroService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CadastroService.Infrastructure.Persistence.Repositories;

public sealed class CadastroRepository(CadastroDbContext db) : ICadastroRepository
{
    public void Add(CadastroData data) => db.Cadastros.Add(new CadastroEntity
    {
        Id = data.CadastroId,
        Username = data.Username,
        Email = data.Email,
        Nome = data.Nome,
        Estado = Steps.Pendente,
        Historico = "[]",
        CriadoEm = DateTimeOffset.UtcNow,
    });

    public async Task<CadastroState?> Get(string cadastroId, CancellationToken ct)
    {
        var e = await db.Cadastros.AsNoTracking().FirstOrDefaultAsync(x => x.Id == cadastroId, ct);
        return e is null ? null : new CadastroState(e.Id, e.Username, e.Email, e.Estado, ReadHistorico(e), e.Motivo, e.KeycloakId);
    }

    /// <summary>FOR UPDATE SKIP LOCKED: trava as linhas escolhidas e pula as que outra transação já travou.</summary>
    public async Task<IReadOnlyList<CadastroData>> ClaimPending(int max, CancellationToken ct)
    {
        var rows = await db.Cadastros
            .FromSqlInterpolated($"SELECT * FROM cadastros WHERE estado = {Steps.Pendente} ORDER BY criado_em LIMIT {max} FOR UPDATE SKIP LOCKED")
            .ToListAsync(ct);
        return rows.Select(e => new CadastroData(e.Id, e.Username, e.Email, e.Nome)).ToList();
    }

    public async Task MarcarIniciado(string cadastroId, CancellationToken ct) =>
        (await Load(cadastroId, ct)).Estado = Steps.Iniciado;

    public async Task RegistrarPasso(string cadastroId, string estado, string? motivo, string? keycloakId, CancellationToken ct)
    {
        var e = await Load(cadastroId, ct);
        var historico = ReadHistorico(e);
        if (historico.Count == 0 || historico[^1] != estado) historico.Add(estado);
        e.Estado = estado;
        e.Historico = JsonSerializer.Serialize(historico);
        e.Motivo = motivo ?? e.Motivo;
        e.KeycloakId = keycloakId ?? e.KeycloakId;
    }

    /// <summary>Carrega a linha para alterar (o EF reaproveita a que o ClaimPending já trouxe no mesmo DbContext).</summary>
    private async Task<CadastroEntity> Load(string cadastroId, CancellationToken ct) =>
        await db.Cadastros.FindAsync([cadastroId], ct) ?? throw new InvalidOperationException($"Cadastro {cadastroId} não encontrado");

    private static List<string> ReadHistorico(CadastroEntity e) => JsonSerializer.Deserialize<List<string>>(e.Historico) ?? [];
}
