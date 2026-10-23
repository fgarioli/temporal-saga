using CadastroService.Core.Domain.Simulations;
using Xunit;

namespace CadastroService.Core.Domain.Tests.Simulations;

public class SimulatedFailuresTests
{
    [Fact]
    public void NasceSemFalhasAgendadas()
    {
        var failures = SimulatedFailures.Create();

        Assert.Equal(SimulatedFailures.SingletonId, failures.Id);
        Assert.Equal(0, failures.Remaining);
        Assert.False(failures.TryConsume());
    }

    [Fact]
    public void ScheduleAcumulaAsFalhasAgendadas()
    {
        var failures = SimulatedFailures.Create();

        failures.Schedule(2);
        failures.Schedule(3);

        Assert.Equal(5, failures.Remaining);
    }

    [Fact]
    public void TryConsumeGastaUmaPorVezAteZerar()
    {
        var failures = SimulatedFailures.Create();
        failures.Schedule(2);

        Assert.True(failures.TryConsume());
        Assert.True(failures.TryConsume());
        Assert.False(failures.TryConsume());
        Assert.Equal(0, failures.Remaining);
    }
}
