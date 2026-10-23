using CadastroService.Core.Domain.Usuarios;
using CadastroService.Infrastructure.Persistence;
using CadastroService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CadastroService.Infrastructure.Tests;

/// <summary>
/// Teste de integração com um Postgres de verdade: docker compose run --rm integration-tests.
/// Sem a variável TEST_POSTGRES, ele não faz nada (é o caso do build da imagem).
/// </summary>
[Trait("Category", "Integration")]
public class OptimisticConcurrencyTests
{
    private static readonly string? ConnectionString = Environment.GetEnvironmentVariable("TEST_POSTGRES");

    private static CadastroDbContext NewContext() =>
        new(new DbContextOptionsBuilder<CadastroDbContext>().UseNpgsql(ConnectionString).Options);

    [Fact]
    public async Task SegundaGravacaoSobreAMesmaVersaoFalha()
    {
        if (ConnectionString is null) return;
        await using (var setup = NewContext())
        {
            await setup.Database.MigrateAsync();
        }
        var id = Guid.NewGuid().ToString();
        await using (var seed = NewContext())
        {
            new UsuarioRepository(seed).Add(Usuario.Create(id, $"teste-{id[..8]}", $"{id[..8]}@exemplo.com", "Teste"));
            await seed.SaveChangesAsync();
        }

        // Duas operações leem a mesma versão do usuário...
        await using var a = NewContext();
        await using var b = NewContext();
        var repoA = new UsuarioRepository(a);
        var repoB = new UsuarioRepository(b);
        var usuarioA = (await repoA.Find(id, default))!;
        var usuarioB = (await repoB.Find(id, default))!;

        // ...a primeira grava e muda a versão (xmin)...
        usuarioA.Ativar("kc-1");
        repoA.Update(usuarioA);
        await a.SaveChangesAsync();

        // ...e a segunda é recusada, em vez de sobrescrever a primeira.
        usuarioB.Rejeitar("concorrente");
        repoB.Update(usuarioB);
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => b.SaveChangesAsync());

        await using var verify = NewContext();
        Assert.Equal(StatusUsuario.Ativo, (await new UsuarioRepository(verify).Find(id, default))!.Status);
    }
}
