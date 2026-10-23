using CadastroService.Core.Domain.Common;
using CadastroService.Core.Domain.Usuarios;
using Xunit;

namespace CadastroService.Core.Domain.Tests.Usuarios;

public class UsuarioTests
{
    private static readonly DateTimeOffset Agora = new(2026, 10, 23, 12, 0, 0, TimeSpan.Zero);

    private static Usuario Novo() => Usuario.Create("u1", "ana", "ana@exemplo.com", "Ana", Agora);

    [Fact]
    public void NasceDesabilitadoNaEtapaPendenteComHistoricoVazio()
    {
        var usuario = Novo();

        Assert.Equal(("u1", "ana", "ana@exemplo.com", "Ana"), (usuario.Id, usuario.Username, usuario.Email, usuario.Nome));
        Assert.False(usuario.Habilitado);
        Assert.Equal(Steps.Pendente, usuario.Etapa);
        Assert.Empty(usuario.Historico);
        Assert.Null(usuario.Motivo);
        Assert.Null(usuario.KeycloakId);
        Assert.Equal(Agora, usuario.CriadoEm);
    }

    [Fact]
    public void MarcarIniciadoMudaAEtapaSemEntrarNoHistorico()
    {
        var usuario = Novo();

        usuario.MarcarIniciado();

        Assert.Equal(Steps.Iniciado, usuario.Etapa);
        Assert.Empty(usuario.Historico);
    }

    [Fact]
    public void RegistrarPassoMudaAEtapaEAcrescentaAoHistorico()
    {
        var usuario = Novo();

        usuario.RegistrarPasso(Steps.CriandoIdentidade);
        usuario.RegistrarPasso(Steps.HabilitandoUsuario);

        Assert.Equal(Steps.HabilitandoUsuario, usuario.Etapa);
        Assert.Equal([Steps.CriandoIdentidade, Steps.HabilitandoUsuario], usuario.Historico);
    }

    [Fact]
    public void RegistrarOMesmoPassoDeNovoNaoDuplicaNoHistorico()
    {
        var usuario = Novo();

        usuario.RegistrarPasso(Steps.CriandoIdentidade);
        usuario.RegistrarPasso(Steps.CriandoIdentidade);

        Assert.Equal([Steps.CriandoIdentidade], usuario.Historico);
        Assert.Equal(Steps.CriandoIdentidade, usuario.Etapa);
    }

    [Fact]
    public void UmaEtapaRepetidaQueNaoEOUltimaEntraDeNovoNoHistorico()
    {
        var usuario = Novo();

        usuario.RegistrarPasso(Steps.CriandoIdentidade);
        usuario.RegistrarPasso(Steps.HabilitandoUsuario);
        usuario.RegistrarPasso(Steps.CriandoIdentidade);

        Assert.Equal([Steps.CriandoIdentidade, Steps.HabilitandoUsuario, Steps.CriandoIdentidade], usuario.Historico);
    }

    [Fact]
    public void MotivoEKeycloakIdNulosMantemOQueJaEstavaGravado()
    {
        var usuario = Novo();

        usuario.RegistrarPasso(Steps.HabilitandoUsuario, keycloakId: "kc-1");
        usuario.RegistrarPasso(Steps.RejeitandoUsuario, motivo: "username em uso");
        usuario.RegistrarPasso(Steps.Cancelado);

        Assert.Equal("kc-1", usuario.KeycloakId);
        Assert.Equal("username em uso", usuario.Motivo);
    }

    [Fact]
    public void MotivoEKeycloakIdInformadosSubstituemOsAnteriores()
    {
        var usuario = Novo();

        usuario.RegistrarPasso(Steps.HabilitandoUsuario, motivo: "a", keycloakId: "kc-1");
        usuario.RegistrarPasso(Steps.Cancelado, motivo: "b", keycloakId: "kc-2");

        Assert.Equal(("b", "kc-2"), (usuario.Motivo, usuario.KeycloakId));
    }

    [Fact]
    public void HabilitarLigaOUsuarioEERepetivel()
    {
        var usuario = Novo();

        usuario.Habilitar();
        usuario.Habilitar();

        Assert.True(usuario.Habilitado);
    }

    [Theory]
    [InlineData(Steps.RejeitandoUsuario)]
    [InlineData(Steps.Cancelado)]
    public void UsuarioRejeitadoNaoPodeSerHabilitado(string etapa)
    {
        var usuario = Novo();
        usuario.RegistrarPasso(etapa, motivo: "recusado");

        Assert.Throws<DomainException>(usuario.Habilitar);
        Assert.False(usuario.Habilitado);
    }

    [Fact]
    public void RejeitarGravaOMotivoEMantemODesabilitado()
    {
        var usuario = Novo();

        usuario.Rejeitar("username em uso");
        usuario.Rejeitar("username em uso");

        Assert.False(usuario.Habilitado);
        Assert.Equal("username em uso", usuario.Motivo);
    }

    [Fact]
    public void UsuarioHabilitadoNaoPodeSerRejeitado()
    {
        var usuario = Novo();
        usuario.Habilitar();

        Assert.Throws<DomainException>(() => usuario.Rejeitar("motivo"));
        Assert.True(usuario.Habilitado);
        Assert.Null(usuario.Motivo);
    }

    [Theory]
    [InlineData(Steps.Pendente)]
    [InlineData(Steps.Iniciado)]
    [InlineData(Steps.CriandoIdentidade)]
    [InlineData(Steps.HabilitandoUsuario)]
    [InlineData(Steps.LiberandoAcesso)]
    [InlineData(Steps.EnviandoBoasVindas)]
    [InlineData(Steps.RejeitandoUsuario)]
    public void UsuarioComASagaEmAndamentoNaoPodeSerExcluido(string etapa)
    {
        var usuario = Novo();
        if (etapa != Steps.Pendente) usuario.RegistrarPasso(etapa);

        var e = Assert.Throws<DomainException>(usuario.GarantirQuePodeSerExcluido);
        Assert.Contains("ainda está em andamento", e.Message);
    }

    [Theory]
    [InlineData(Steps.Concluido)]
    [InlineData(Steps.Cancelado)]
    public void UsuarioComASagaTerminadaPodeSerExcluido(string etapa)
    {
        var usuario = Novo();
        usuario.RegistrarPasso(etapa);

        usuario.GarantirQuePodeSerExcluido();
    }

    [Fact]
    public void ToDataEspelhaOUsuario()
    {
        Assert.Equal(new UsuarioData("u1", "ana", "ana@exemplo.com", "Ana"), Novo().ToData());
    }
}
