using CadastroService.Core.Application.Features.Cadastrar;
using CadastroService.Core.Domain.Cadastros;
using CadastroService.Core.Domain.Common;
using CadastroService.Core.Domain.Usuarios;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CadastroService.Core.Application.Tests.Features.Cadastrar;

public class CadastrarHandlerTests
{
    private static readonly CadastrarCommand Ana = new("ana", "ana@x.com", "Ana Souza");

    /// <summary>O que foi adicionado, para o teste conferir o que cada repositório recebeu e quando.</summary>
    private sealed class Gravacao
    {
        public List<Cadastro> Cadastros { get; } = [];
        public List<Usuario> Usuarios { get; } = [];
        public List<(int Cadastros, int Usuarios)> NoSaveChanges { get; } = [];
    }

    private sealed class FakeCadastroRepository(Gravacao gravacao) : ICadastroRepository
    {
        public void Add(Cadastro cadastro) => gravacao.Cadastros.Add(cadastro);
        public Task<Cadastro?> Get(string cadastroId, CancellationToken ct) => throw new NotSupportedException();
        public Task<IReadOnlyList<Cadastro>> ClaimPending(int max, CancellationToken ct) => throw new NotSupportedException();
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
    }

    private sealed class FakeUnitOfWork(Gravacao gravacao) : IUnitOfWork
    {
        public bool ThrowUniqueViolation { get; init; }

        public Task SaveChanges(CancellationToken ct)
        {
            gravacao.NoSaveChanges.Add((gravacao.Cadastros.Count, gravacao.Usuarios.Count));
            if (ThrowUniqueViolation) throw new UniqueConstraintViolationException("ix_usuarios_username", new InvalidOperationException("23505"));
            return Task.CompletedTask;
        }

        public Task<ITransaction> BeginTransaction(CancellationToken ct) => throw new NotSupportedException();
    }

    private static CadastrarHandler NewHandler(Gravacao gravacao, FakeUsuarioRepository? usuarios = null, FakeUnitOfWork? unitOfWork = null) =>
        new(new FakeCadastroRepository(gravacao), usuarios ?? new FakeUsuarioRepository(gravacao), unitOfWork ?? new FakeUnitOfWork(gravacao),
            NullLogger<CadastrarHandler>.Instance);

    [Fact]
    public async Task GravaOUsuarioPendenteEOCadastroPendenteNumaUnicaChamadaDeSaveChanges()
    {
        var gravacao = new Gravacao();

        var result = await NewHandler(gravacao).Handle(Ana, default);

        var created = Assert.IsType<CadastroCreationResult.Created>(result);
        Assert.Equal([(1, 1)], gravacao.NoSaveChanges);
        var cadastro = Assert.Single(gravacao.Cadastros);
        var usuario = Assert.Single(gravacao.Usuarios);
        Assert.Equal(Steps.Pendente, cadastro.Estado);
        Assert.Equal(StatusUsuario.Pendente, usuario.Status);
        Assert.Equal(cadastro.Id, usuario.Id);
        Assert.Equal(("ana", "ana@x.com", "Ana Souza"), (usuario.Username, usuario.Email, usuario.Nome));
        Assert.Equal(cadastro.Id, created.State.CadastroId);
        Assert.Equal(Steps.Pendente, created.State.Estado);
    }

    [Fact]
    public async Task UsernameEmUsoDevolveARecusaSemGravarNada()
    {
        var gravacao = new Gravacao();
        var usuarios = new FakeUsuarioRepository(gravacao) { UsernameEmUso = true };

        var result = await NewHandler(gravacao, usuarios).Handle(Ana, default);

        var recusa = Assert.IsType<CadastroCreationResult.UsernameEmUso>(result);
        Assert.Equal("O username ana já está em uso", recusa.Mensagem);
        Assert.Equal("ana", usuarios.UsernameConsultado);
        Assert.Empty(gravacao.Cadastros);
        Assert.Empty(gravacao.Usuarios);
        Assert.Empty(gravacao.NoSaveChanges);
    }

    [Fact]
    public async Task ViolacaoDeUnicidadeNaGravacaoViraUsernameEmUso()
    {
        var gravacao = new Gravacao();
        var unitOfWork = new FakeUnitOfWork(gravacao) { ThrowUniqueViolation = true };

        var result = await NewHandler(gravacao, unitOfWork: unitOfWork).Handle(Ana, default);

        var recusa = Assert.IsType<CadastroCreationResult.UsernameEmUso>(result);
        Assert.Equal("O username ana já está em uso", recusa.Mensagem);
        Assert.Single(gravacao.NoSaveChanges);
    }
}
