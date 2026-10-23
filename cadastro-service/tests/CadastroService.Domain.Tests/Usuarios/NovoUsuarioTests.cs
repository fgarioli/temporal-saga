using CadastroService.Core.Domain.Usuarios;
using Xunit;

namespace CadastroService.Core.Domain.Tests.Usuarios;

public class NovoUsuarioTests
{
    [Fact]
    public void NormalizaOPedido()
    {
        var request = NovoUsuario.Create(" Ana ", "ANA@exemplo.com", " Ana Souza ");
        Assert.Equal(("ana", "ana@exemplo.com", "Ana Souza"), (request.Username, request.Email, request.Nome));
    }

    [Theory]
    [InlineData("a", "a@x.com", "Ana")]
    [InlineData("ana", "sem-arroba", "Ana")]
    [InlineData("ana", "a@x.com", " ")]
    public void RecusaPedidoInvalido(string username, string email, string nome) =>
        Assert.Throws<ArgumentException>(() => NovoUsuario.Create(username, email, nome));
}
