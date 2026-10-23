using CadastroService.Core.Domain.Simulations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CadastroService.Infrastructure.Persistence.Configurations;

public sealed class SimulatedFailuresConfiguration : IEntityTypeConfiguration<SimulatedFailures>
{
    public void Configure(EntityTypeBuilder<SimulatedFailures> b)
    {
        b.ToTable("simulated_failures");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.Remaining).HasColumnName("remaining");
    }
}
