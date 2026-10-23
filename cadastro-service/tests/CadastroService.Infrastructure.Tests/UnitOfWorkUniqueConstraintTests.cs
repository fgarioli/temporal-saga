using CadastroService.Core.Domain.Common;
using CadastroService.Core.Domain.Usuarios;
using CadastroService.Infrastructure.Persistence;
using CadastroService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace CadastroService.Infrastructure.Tests;

/// <summary>
/// Teste de integração com um Postgres de verdade: docker compose run --rm integration-tests.
/// Sem a variável TEST_POSTGRES, ele não faz nada (é o caso do build da imagem).
/// </summary>
[Trait("Category", "Integration")]
public class UnitOfWorkUniqueConstraintTests
{
    private static readonly string? ConnectionString = Environment.GetEnvironmentVariable("TEST_POSTGRES");

    private static CadastroDbContext NewContext() =>
        new(new DbContextOptionsBuilder<CadastroDbContext>().UseNpgsql(ConnectionString).Options);

    [Fact]
    public async Task SegundoUsuarioPendenteComOMesmoUsernameLancaUniqueConstraintViolationException()
    {
        if (ConnectionString is null) return;
        await using (var setup = NewContext())
        {
            await setup.Database.MigrateAsync();
        }
        var username = $"teste-{Guid.NewGuid().ToString()[..8]}";

        // Dois cadastros disputam o mesmo username: cada um com o próprio DbContext, como em duas requisições simultâneas.
        await using var a = NewContext();
        await using var b = NewContext();
        new UsuarioRepository(a).Add(Usuario.Create(Guid.NewGuid().ToString(), username, $"{username}@a.com", "Ana"));
        new UsuarioRepository(b).Add(Usuario.Create(Guid.NewGuid().ToString(), username, $"{username}@b.com", "Ana"));

        // O primeiro grava; o segundo é recusado pelo índice único, com a exceção do domínio, sem vazar a do EF Core.
        await new EfUnitOfWork(a).SaveChanges(default);
        var e = await Assert.ThrowsAsync<UniqueConstraintViolationException>(() => new EfUnitOfWork(b).SaveChanges(default));
        Assert.Equal("23505", Assert.IsType<PostgresException>(e.InnerException!.InnerException).SqlState);

        await using var verify = NewContext();
        Assert.Equal(1, await verify.Usuarios.CountAsync(u => u.Username == username));
    }
}
