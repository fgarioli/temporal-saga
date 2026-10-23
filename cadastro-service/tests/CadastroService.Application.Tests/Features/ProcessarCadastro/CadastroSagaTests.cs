using CadastroService.Core.Domain.Cadastros;
using CadastroService.Core.Application.Features.ProcessarCadastro;
using Xunit;

namespace CadastroService.Core.Application.Tests.Features.ProcessarCadastro;

public class CadastroSagaTests
{
    private static readonly CadastroData Ana = new("c-1", "ana", "ana@x.com", "Ana Souza");

    /// <summary>Registra cada passo chamado e o que a saga gravou; recusa o passo que o teste mandar.</summary>
    private sealed class FakeSteps : ICadastroSagaSteps
    {
        public List<string> Calls { get; } = [];
        public List<string> Historico { get; } = [];
        public string? Estado { get; private set; }
        public string? Motivo { get; private set; }
        public string? KeycloakId { get; private set; }
        public string? RecusarEm { get; init; }

        public async Task<string> CriarNoKeycloak(CadastroData data)
        {
            await Run(nameof(CriarNoKeycloak));
            return "kc-ana";
        }

        public Task AtivarUsuario(string cadastroId, string keycloakId) => Run(nameof(AtivarUsuario));

        public Task RejeitarUsuario(string cadastroId, string motivo) => Run($"{nameof(RejeitarUsuario)}({motivo})");

        public Task EnviarBoasVindas(CadastroData data) => Run(nameof(EnviarBoasVindas));

        public Task RegistrarPasso(string cadastroId, string estado, string? motivo = null, string? keycloakId = null)
        {
            Estado = estado;
            Historico.Add(estado);
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
    public async Task CaminhoFelizPercorreOsTresPassosAteConcluido()
    {
        var steps = new FakeSteps();

        var final = await new CadastroSaga(steps).Run(Ana);

        Assert.Equal(Steps.Concluido, final);
        Assert.Equal(Steps.Concluido, steps.Estado);
        Assert.Equal(["CriandoIdentidade", "AtivandoUsuario", "EnviandoBoasVindas", "Concluido"], steps.Historico);
        Assert.Equal(["CriarNoKeycloak", "AtivarUsuario", "EnviarBoasVindas"], steps.Calls);
        Assert.Equal("kc-ana", steps.KeycloakId);
        Assert.Null(steps.Motivo);
    }

    [Fact]
    public async Task RecusaNoPivoCompensaRejeitandoOUsuarioECancela()
    {
        var steps = new FakeSteps { RecusarEm = "CriarNoKeycloak" };

        var final = await new CadastroSaga(steps).Run(Ana);

        Assert.Equal(Steps.Cancelado, final);
        Assert.Equal(Steps.Cancelado, steps.Estado);
        Assert.Equal(["CriandoIdentidade", "RejeitandoUsuario", "Cancelado"], steps.Historico);
        Assert.Equal(["CriarNoKeycloak", "RejeitarUsuario(recusado em CriarNoKeycloak)"], steps.Calls);
        Assert.Equal("recusado em CriarNoKeycloak", steps.Motivo);
        Assert.Null(steps.KeycloakId);
    }
}
