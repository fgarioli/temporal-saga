using CadastroService.Core.Domain.Cadastros;
using CadastroService.Core.Domain.Envios;
using CadastroService.Core.Domain.Simulations;
using CadastroService.Core.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace CadastroService.Infrastructure.Persistence;

/// <summary>O EF Core mapeia as entidades do Domain direto; o mapeamento de cada uma fica em Configurations.</summary>
public sealed class CadastroDbContext(DbContextOptions<CadastroDbContext> options) : DbContext(options)
{
    public DbSet<Cadastro> Cadastros => Set<Cadastro>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Envio> Envios => Set<Envio>();
    public DbSet<SimulatedFailures> SimulatedFailures => Set<SimulatedFailures>();

    protected override void OnModelCreating(ModelBuilder model) =>
        model.ApplyConfigurationsFromAssembly(typeof(CadastroDbContext).Assembly);
}
