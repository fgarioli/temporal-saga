using CadastroService.Core.Domain.Cadastros;
using Xunit;

namespace CadastroService.Core.Domain.Tests.Cadastros;

public class CadastroTests
{
    private static readonly DateTimeOffset Agora = new(2026, 10, 23, 12, 0, 0, TimeSpan.Zero);

    private static Cadastro Novo() => Cadastro.Create("c1", "ana", "ana@exemplo.com", "Ana", Agora);

    [Fact]
    public void NasceNoEstadoPendenteComHistoricoVazio()
    {
        var cadastro = Novo();

        Assert.Equal(("c1", "ana", "ana@exemplo.com", "Ana"), (cadastro.Id, cadastro.Username, cadastro.Email, cadastro.Nome));
        Assert.Equal(Steps.Pendente, cadastro.Estado);
        Assert.Empty(cadastro.Historico);
        Assert.Null(cadastro.Motivo);
        Assert.Null(cadastro.KeycloakId);
        Assert.Equal(Agora, cadastro.CriadoEm);
    }

    [Fact]
    public void MarcarIniciadoMudaOEstadoSemEntrarNoHistorico()
    {
        var cadastro = Novo();

        cadastro.MarcarIniciado();

        Assert.Equal(Steps.Iniciado, cadastro.Estado);
        Assert.Empty(cadastro.Historico);
    }

    [Fact]
    public void RegistrarPassoMudaOEstadoEAcrescentaAoHistorico()
    {
        var cadastro = Novo();

        cadastro.RegistrarPasso(Steps.CriandoUsuario);
        cadastro.RegistrarPasso(Steps.CriandoIdentidade);

        Assert.Equal(Steps.CriandoIdentidade, cadastro.Estado);
        Assert.Equal([Steps.CriandoUsuario, Steps.CriandoIdentidade], cadastro.Historico);
    }

    [Fact]
    public void RegistrarOMesmoPassoDeNovoNaoDuplicaNoHistorico()
    {
        var cadastro = Novo();

        cadastro.RegistrarPasso(Steps.CriandoUsuario);
        cadastro.RegistrarPasso(Steps.CriandoUsuario);

        Assert.Equal([Steps.CriandoUsuario], cadastro.Historico);
        Assert.Equal(Steps.CriandoUsuario, cadastro.Estado);
    }

    [Fact]
    public void UmEstadoRepetidoQueNaoEOUltimoEntraDeNovoNoHistorico()
    {
        var cadastro = Novo();

        cadastro.RegistrarPasso(Steps.CriandoUsuario);
        cadastro.RegistrarPasso(Steps.CriandoIdentidade);
        cadastro.RegistrarPasso(Steps.CriandoUsuario);

        Assert.Equal([Steps.CriandoUsuario, Steps.CriandoIdentidade, Steps.CriandoUsuario], cadastro.Historico);
    }

    [Fact]
    public void MotivoEKeycloakIdNulosMantemOQueJaEstavaGravado()
    {
        var cadastro = Novo();

        cadastro.RegistrarPasso(Steps.AtivandoUsuario, keycloakId: "kc-1");
        cadastro.RegistrarPasso(Steps.RejeitandoUsuario, motivo: "username em uso");
        cadastro.RegistrarPasso(Steps.Cancelado);

        Assert.Equal("kc-1", cadastro.KeycloakId);
        Assert.Equal("username em uso", cadastro.Motivo);
    }

    [Fact]
    public void MotivoEKeycloakIdInformadosSubstituemOsAnteriores()
    {
        var cadastro = Novo();

        cadastro.RegistrarPasso(Steps.AtivandoUsuario, motivo: "a", keycloakId: "kc-1");
        cadastro.RegistrarPasso(Steps.Cancelado, motivo: "b", keycloakId: "kc-2");

        Assert.Equal(("b", "kc-2"), (cadastro.Motivo, cadastro.KeycloakId));
    }

    [Fact]
    public void ToDataEToStateEspelhamOCadastro()
    {
        var cadastro = Novo();
        cadastro.RegistrarPasso(Steps.AtivandoUsuario, keycloakId: "kc-1");

        Assert.Equal(new CadastroData("c1", "ana", "ana@exemplo.com", "Ana"), cadastro.ToData());

        var state = cadastro.ToState();
        Assert.Equal(("c1", "ana", "ana@exemplo.com", Steps.AtivandoUsuario, null, "kc-1"),
            (state.CadastroId, state.Username, state.Email, state.Estado, state.Motivo, state.KeycloakId));
        Assert.Equal([Steps.AtivandoUsuario], state.Historico);
    }
}
