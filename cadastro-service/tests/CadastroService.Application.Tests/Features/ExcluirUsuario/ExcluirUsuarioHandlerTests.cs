using CadastroService.Application.Features.ExcluirUsuario;
using CadastroService.Domain.Common;
using CadastroService.Domain.Usuarios;
using Xunit;

namespace CadastroService.Application.Tests.Features.ExcluirUsuario;

public class ExcluirUsuarioHandlerTests
{
    private sealed class FakeUsuarioRepository : IUsuarioRepository
    {
        public Dictionary<string, Usuario> Usuarios { get; } = [];
        public List<Usuario> Removidos { get; } = [];

        public Task<Usuario?> Find(string id, CancellationToken ct) => Task.FromResult(Usuarios.GetValueOrDefault(id));
        public Task<bool> IsUsernameInUse(string username, string exceptId, CancellationToken ct) => throw new NotSupportedException();
        public void Add(Usuario usuario) => throw new NotSupportedException();
        public void Update(Usuario usuario) => throw new NotSupportedException();
        public void Remove(Usuario usuario) => Removidos.Add(usuario);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCalls { get; private set; }
        public bool ThrowConflict { get; init; }

        public Task SaveChanges(CancellationToken ct)
        {
            SaveChangesCalls++;
            if (ThrowConflict) throw new ConcurrencyConflictException("versão mudou", new InvalidOperationException("xmin"));
            return Task.CompletedTask;
        }

        public Task<ITransaction> BeginTransaction(CancellationToken ct) => throw new NotSupportedException();
    }

    private static Usuario Ativo(string id)
    {
        var usuario = Usuario.Create(id, "ana", "ana@x.com", "Ana Souza");
        usuario.Ativar("kc-ana");
        return usuario;
    }

    [Fact]
    public async Task UsuarioInexistenteDevolveNotFoundSemGravar()
    {
        var repo = new FakeUsuarioRepository();
        var uow = new FakeUnitOfWork();

        var result = await new ExcluirUsuarioHandler(repo, uow).Handle("u-1", default);

        Assert.Equal(UsuarioDeletionStatus.NotFound, result.Status);
        Assert.Empty(repo.Removidos);
        Assert.Equal(0, uow.SaveChangesCalls);
    }

    [Fact]
    public async Task CadastroEmAndamentoDevolveARecusaComAMensagemSemGravar()
    {
        var repo = new FakeUsuarioRepository();
        repo.Usuarios["u-1"] = Usuario.Create("u-1", "ana", "ana@x.com", "Ana Souza");
        var uow = new FakeUnitOfWork();

        var result = await new ExcluirUsuarioHandler(repo, uow).Handle("u-1", default);

        Assert.Equal(UsuarioDeletionStatus.CadastroEmAndamento, result.Status);
        Assert.Contains("ainda está em andamento", result.Mensagem);
        Assert.Empty(repo.Removidos);
        Assert.Equal(0, uow.SaveChangesCalls);
    }

    [Fact]
    public async Task ConflitoDeVersaoNaGravacaoDevolveConflict()
    {
        var repo = new FakeUsuarioRepository();
        repo.Usuarios["u-1"] = Ativo("u-1");
        var uow = new FakeUnitOfWork { ThrowConflict = true };

        var result = await new ExcluirUsuarioHandler(repo, uow).Handle("u-1", default);

        Assert.Equal(UsuarioDeletionStatus.Conflict, result.Status);
        Assert.Equal(1, uow.SaveChangesCalls);
    }

    [Fact]
    public async Task UsuarioAtivoERemovidoEGravado()
    {
        var repo = new FakeUsuarioRepository();
        var usuario = Ativo("u-1");
        repo.Usuarios["u-1"] = usuario;
        var uow = new FakeUnitOfWork();

        var result = await new ExcluirUsuarioHandler(repo, uow).Handle("u-1", default);

        Assert.Equal(UsuarioDeletionStatus.Deleted, result.Status);
        Assert.Same(usuario, Assert.Single(repo.Removidos));
        Assert.Equal(1, uow.SaveChangesCalls);
    }
}
