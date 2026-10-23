using CadastroService.Core.Application.Features.CriarUsuario;
using CadastroService.Core.Domain.Common;
using CadastroService.Core.Domain.Usuarios;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CadastroService.Core.Application.Tests.Features.CriarUsuario;

public class CriarUsuarioHandlerTests
{
    private static readonly CriarUsuarioCommand Ana = new("ana", "ana@x.com", "Ana Souza");

    /// <summary>O que foi adicionado, para o teste conferir o que o repositório recebeu e quando.</summary>
    private sealed class Gravacao
    {
        public List<Usuario> Usuarios { get; } = [];
        public List<int> NoSaveChanges { get; } = [];
    }

    private sealed class FakeUsuarioRepository(Gravacao gravacao) : IUsuarioRepository
    {
        public bool UsernameEmUso { get; init; }
        public string? UsernameConsultado { get; private set; }

        public Task<Usuario?> Find(string id, CancellationToken ct) => throw new NotSupportedException();

        public Task<bool> IsUsernameInUse(string username, string exceptId, CancellationToken ct)
        {
            UsernameConsultado = username;
            return Task.FromResult(UsernameEmUso);
        }

        public void Add(Usuario usuario) => gravacao.Usuarios.Add(usuario);
        public void Update(Usuario usuario) => throw new NotSupportedException();
        public void Remove(Usuario usuario) => throw new NotSupportedException();
        public Task<IReadOnlyList<Usuario>> ClaimPending(int max, CancellationToken ct) => throw new NotSupportedException();
    }

    private sealed class FakeUnitOfWork(Gravacao gravacao) : IUnitOfWork
    {
        public bool ThrowUniqueViolation { get; init; }

        public Task SaveChanges(CancellationToken ct)
        {
            gravacao.NoSaveChanges.Add(gravacao.Usuarios.Count);
            if (ThrowUniqueViolation) throw new UniqueConstraintViolationException("ix_usuarios_username", new InvalidOperationException("23505"));
            return Task.CompletedTask;
        }

        public Task<ITransaction> BeginTransaction(CancellationToken ct) => throw new NotSupportedException();
    }

    private static CriarUsuarioHandler NewHandler(Gravacao gravacao, FakeUsuarioRepository? usuarios = null, FakeUnitOfWork? unitOfWork = null) =>
        new(usuarios ?? new FakeUsuarioRepository(gravacao), unitOfWork ?? new FakeUnitOfWork(gravacao), NullLogger<CriarUsuarioHandler>.Instance);

    [Fact]
    public async Task GravaOUsuarioPendenteEDesabilitadoNumaUnicaChamadaDeSaveChanges()
    {
        var gravacao = new Gravacao();

        var result = await NewHandler(gravacao).Handle(Ana, default);

        var created = Assert.IsType<UsuarioCreationResult.Created>(result);
        Assert.Equal([1], gravacao.NoSaveChanges);
        var usuario = Assert.Single(gravacao.Usuarios);
        Assert.Same(usuario, created.Usuario);
        Assert.Equal(Steps.Pendente, usuario.Etapa);
        Assert.False(usuario.Habilitado);
        Assert.Empty(usuario.Historico);
        Assert.Equal(("ana", "ana@x.com", "Ana Souza"), (usuario.Username, usuario.Email, usuario.Nome));
    }

    [Fact]
    public async Task UsernameEmUsoDevolveARecusaSemGravarNada()
    {
        var gravacao = new Gravacao();
        var usuarios = new FakeUsuarioRepository(gravacao) { UsernameEmUso = true };

        var result = await NewHandler(gravacao, usuarios).Handle(Ana, default);

        var recusa = Assert.IsType<UsuarioCreationResult.UsernameEmUso>(result);
        Assert.Equal("O username ana já está em uso", recusa.Mensagem);
        Assert.Equal("ana", usuarios.UsernameConsultado);
        Assert.Empty(gravacao.Usuarios);
        Assert.Empty(gravacao.NoSaveChanges);
    }

    [Fact]
    public async Task ViolacaoDeUnicidadeNaGravacaoViraUsernameEmUso()
    {
        var gravacao = new Gravacao();
        var unitOfWork = new FakeUnitOfWork(gravacao) { ThrowUniqueViolation = true };

        var result = await NewHandler(gravacao, unitOfWork: unitOfWork).Handle(Ana, default);

        var recusa = Assert.IsType<UsuarioCreationResult.UsernameEmUso>(result);
        Assert.Equal("O username ana já está em uso", recusa.Mensagem);
        Assert.Single(gravacao.NoSaveChanges);
    }
}
