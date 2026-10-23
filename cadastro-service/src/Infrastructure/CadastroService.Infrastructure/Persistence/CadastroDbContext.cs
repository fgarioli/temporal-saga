using CadastroService.Core.Domain.Usuarios;
using CadastroService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CadastroService.Infrastructure.Persistence;

public sealed class CadastroDbContext(DbContextOptions<CadastroDbContext> options) : DbContext(options)
{
    public DbSet<CadastroEntity> Cadastros => Set<CadastroEntity>();
    public DbSet<UsuarioEntity> Usuarios => Set<UsuarioEntity>();
    public DbSet<EnvioEntity> Envios => Set<EnvioEntity>();
    public DbSet<SimulatedFailuresEntity> SimulatedFailures => Set<SimulatedFailuresEntity>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<CadastroEntity>(b =>
        {
            b.ToTable("cadastros");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Username).HasColumnName("username");
            b.Property(x => x.Email).HasColumnName("email");
            b.Property(x => x.Nome).HasColumnName("nome");
            b.Property(x => x.Estado).HasColumnName("estado");
            b.Property(x => x.Historico).HasColumnName("historico").HasColumnType("jsonb");
            b.Property(x => x.Motivo).HasColumnName("motivo");
            b.Property(x => x.KeycloakId).HasColumnName("keycloak_id");
            b.Property(x => x.CriadoEm).HasColumnName("criado_em");
            // O dispatcher busca os pendentes por estado.
            b.HasIndex(x => x.Estado);
        });
        model.Entity<UsuarioEntity>(b =>
        {
            b.ToTable("usuarios");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Username).HasColumnName("username");
            b.Property(x => x.Email).HasColumnName("email");
            b.Property(x => x.Nome).HasColumnName("nome");
            b.Property(x => x.Status).HasColumnName("status");
            b.Property(x => x.KeycloakId).HasColumnName("keycloak_id");
            b.Property(x => x.Motivo).HasColumnName("motivo");
            b.Property(x => x.Version).IsRowVersion();
            // Um username só pode estar em uso por um cadastro não rejeitado.
            b.HasIndex(x => x.Username).IsUnique().HasFilter($"status <> '{nameof(StatusUsuario.Rejeitado)}'");
        });
        model.Entity<EnvioEntity>(b =>
        {
            b.ToTable("envios");
            b.HasKey(x => x.CadastroId);
            b.Property(x => x.CadastroId).HasColumnName("cadastro_id");
            b.Property(x => x.Email).HasColumnName("email");
            b.Property(x => x.EnviadoEm).HasColumnName("enviado_em");
        });
        model.Entity<SimulatedFailuresEntity>(b =>
        {
            b.ToTable("simulated_failures");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            b.Property(x => x.Remaining).HasColumnName("remaining");
        });
    }
}
