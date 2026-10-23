using CadastroService.Core.Domain.Common;
using CadastroService.Core.Domain.Usuarios;
using Xunit;

namespace CadastroService.Core.Domain.Tests;

public class DomainTests
{
    [Fact]
    public void UsuarioPendenteNaoPodeSerExcluido()
    {
        var usuario = Usuario.Create("u1", "ana", "ana@exemplo.com", "Ana");
        Assert.Throws<DomainException>(usuario.GarantirQuePodeSerExcluido);
        usuario.Ativar("kc-1");
        usuario.GarantirQuePodeSerExcluido();
    }

    [Fact]
    public void AtivarERejeitarSaoIdempotentes()
    {
        var ativo = Usuario.Create("u1", "ana", "ana@exemplo.com", "Ana");
        ativo.Ativar("kc-1");
        ativo.Ativar("kc-1");
        Assert.Equal(StatusUsuario.Ativo, ativo.Status);
        Assert.Throws<DomainException>(() => ativo.Rejeitar("motivo"));

        var rejeitado = Usuario.Create("u2", "bia", "bia@exemplo.com", "Bia");
        rejeitado.Rejeitar("motivo");
        rejeitado.Rejeitar("outro");
        Assert.Equal("motivo", rejeitado.Motivo);
        Assert.Throws<DomainException>(() => rejeitado.Ativar("kc-2"));
    }

    [Fact]
    public void NormalizaOPedido()
    {
        var request = NovoCadastro.Create(" Ana ", "ANA@exemplo.com", " Ana Souza ");
        Assert.Equal(("ana", "ana@exemplo.com", "Ana Souza"), (request.Username, request.Email, request.Nome));
    }

    [Theory]
    [InlineData("a", "a@x.com", "Ana")]
    [InlineData("ana", "sem-arroba", "Ana")]
    [InlineData("ana", "a@x.com", " ")]
    public void RecusaPedidoInvalido(string username, string email, string nome) =>
        Assert.Throws<ArgumentException>(() => NovoCadastro.Create(username, email, nome));
}
