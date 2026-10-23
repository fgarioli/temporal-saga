using CadastroService.Core.Domain.Simulations;
using Xunit;

namespace CadastroService.Core.Domain.Tests.Simulations;

public class SimulatedFailuresTests
{
    [Fact]
    public void NasceSemFalhasAgendadasParaOAlvo()
    {
        var failures = SimulatedFailures.Create(SimulatedFailures.KeycloakEnable);

        Assert.Equal(SimulatedFailures.KeycloakEnable, failures.Id);
        Assert.Equal(0, failures.Remaining);
        Assert.False(failures.TryConsume());
    }

    [Fact]
    public void ScheduleAcumulaAsFalhasAgendadas()
    {
        var failures = SimulatedFailures.Create(SimulatedFailures.KeycloakCreate);

        failures.Schedule(2);
        failures.Schedule(3);

        Assert.Equal(5, failures.Remaining);
    }

    [Fact]
    public void TryConsumeGastaUmaPorVezAteZerar()
    {
        var failures = SimulatedFailures.Create(SimulatedFailures.KeycloakCreate);
        failures.Schedule(2);

        Assert.True(failures.TryConsume());
        Assert.True(failures.TryConsume());
        Assert.False(failures.TryConsume());
        Assert.Equal(0, failures.Remaining);
    }

    [Theory]
    [InlineData(SimulatedFailures.KeycloakCreate, true)]
    [InlineData(SimulatedFailures.KeycloakEnable, true)]
    [InlineData("keycloak-delete", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void SoOsAlvosConhecidosSaoAceitos(string? alvo, bool conhecido) =>
        Assert.Equal(conhecido, SimulatedFailures.IsKnown(alvo));
}
