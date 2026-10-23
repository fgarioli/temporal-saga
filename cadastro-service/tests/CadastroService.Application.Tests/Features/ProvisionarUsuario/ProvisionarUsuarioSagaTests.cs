using CadastroService.Core.Application.Features.ProvisionarUsuario;
using CadastroService.Core.Domain.Usuarios;
using Xunit;

namespace CadastroService.Core.Application.Tests.Features.ProvisionarUsuario;

public class ProvisionarUsuarioSagaTests
{
    private static readonly UsuarioData Ana = new("u-1", "ana", "ana@x.com", "Ana Souza");

    /// <summary>Registra cada passo chamado e o que a saga gravou; recusa o passo que o teste mandar.</summary>
    private sealed class FakeSteps : IProvisionarUsuarioSteps
    {
        public List<string> Calls { get; } = [];
        public List<string> Historico { get; } = [];
        public string? Etapa { get; private set; }
        public string? Motivo { get; private set; }
        public string? KeycloakId { get; private set; }
        public string? RecusarEm { get; init; }

        public async Task<string> CriarNoKeycloak(UsuarioData data)
        {
            await Run(nameof(CriarNoKeycloak));
            return "kc-ana";
        }

        public Task HabilitarUsuario(string usuarioId) => Run(nameof(HabilitarUsuario));

        public Task LiberarAcesso(string keycloakId) => Run($"{nameof(LiberarAcesso)}({keycloakId})");

        public Task RejeitarUsuario(string usuarioId, string motivo) => Run($"{nameof(RejeitarUsuario)}({motivo})");

        public Task EnviarBoasVindas(UsuarioData data) => Run(nameof(EnviarBoasVindas));

        public Task RegistrarPasso(string usuarioId, string etapa, string? motivo = null, string? keycloakId = null)
        {
            Etapa = etapa;
            Historico.Add(etapa);
            Motivo = motivo ?? Motivo;
            KeycloakId = keycloakId ?? KeycloakId;
            return Task.CompletedTask;
        }

        private Task Run(string call)
        {
            Calls.Add(call);
            if (call == RecusarEm) throw new StepRefusedException($"recusado em {call}");
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task CaminhoFelizPercorreOsPassosAteConcluidoLiberandoOAcessoDepoisDeHabilitar()
    {
        var steps = new FakeSteps();

        var final = await new ProvisionarUsuarioSaga(steps).Run(Ana);

        Assert.Equal(Steps.Concluido, final);
        Assert.Equal(Steps.Concluido, steps.Etapa);
        Assert.Equal(["CriandoIdentidade", "HabilitandoUsuario", "LiberandoAcesso", "EnviandoBoasVindas", "Concluido"], steps.Historico);
        Assert.Equal(["CriarNoKeycloak", "HabilitarUsuario", "LiberarAcesso(kc-ana)", "EnviarBoasVindas"], steps.Calls);
        Assert.Equal("kc-ana", steps.KeycloakId);
        Assert.Null(steps.Motivo);
    }

    [Fact]
    public async Task RecusaNoPivoCompensaRejeitandoOUsuarioECancelaSemLiberarOAcesso()
    {
        var steps = new FakeSteps { RecusarEm = "CriarNoKeycloak" };

        var final = await new ProvisionarUsuarioSaga(steps).Run(Ana);

        Assert.Equal(Steps.Cancelado, final);
        Assert.Equal(Steps.Cancelado, steps.Etapa);
        Assert.Equal(["CriandoIdentidade", "RejeitandoUsuario", "Cancelado"], steps.Historico);
        Assert.Equal(["CriarNoKeycloak", "RejeitarUsuario(recusado em CriarNoKeycloak)"], steps.Calls);
        Assert.Equal("recusado em CriarNoKeycloak", steps.Motivo);
        Assert.Null(steps.KeycloakId);
    }
}
