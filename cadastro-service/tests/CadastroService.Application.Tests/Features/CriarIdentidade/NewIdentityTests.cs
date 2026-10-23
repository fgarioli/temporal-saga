using CadastroService.Core.Application.Features.CriarIdentidade;
using Xunit;

namespace CadastroService.Core.Application.Tests.Features.CriarIdentidade;

public class NewIdentityTests
{
    [Fact]
    public void DivideONomeComoOKeycloakEspera()
    {
        var completo = new NewIdentity("ana", "a@x.com", "Ana Maria Souza");
        Assert.Equal(("Ana", "Maria Souza"), (completo.PrimeiroNome, completo.Sobrenome));
        var so = new NewIdentity("bia", "b@x.com", "Bia");
        Assert.Equal(("Bia", "Bia"), (so.PrimeiroNome, so.Sobrenome));
    }
}
