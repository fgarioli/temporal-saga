using CadastroService.Core.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CadastroService.Infrastructure.Persistence.Configurations;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> b)
    {
        b.ToTable("usuarios");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.Username).HasColumnName("username");
        b.Property(x => x.Email).HasColumnName("email");
        b.Property(x => x.Nome).HasColumnName("nome");
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>();
        b.Property(x => x.KeycloakId).HasColumnName("keycloak_id");
        b.Property(x => x.Motivo).HasColumnName("motivo");
        // Concorrência otimista: o Postgres muda o xmin a cada atualização da linha. É uma propriedade sombra: o domínio não a conhece.
        b.Property<uint>("xmin").HasColumnName("xmin").HasColumnType("xid").ValueGeneratedOnAddOrUpdate().IsConcurrencyToken();
        // Um username só pode estar em uso por um cadastro não rejeitado.
        b.HasIndex(x => x.Username).IsUnique().HasFilter($"status <> '{nameof(StatusUsuario.Rejeitado)}'");
    }
}
