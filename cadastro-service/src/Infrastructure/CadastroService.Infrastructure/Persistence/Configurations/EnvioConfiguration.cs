using CadastroService.Core.Domain.Envios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CadastroService.Infrastructure.Persistence.Configurations;

public sealed class EnvioConfiguration : IEntityTypeConfiguration<Envio>
{
    public void Configure(EntityTypeBuilder<Envio> b)
    {
        b.ToTable("envios");
        b.HasKey(x => x.UsuarioId);
        b.Property(x => x.UsuarioId).HasColumnName("usuario_id");
        b.Property(x => x.Email).HasColumnName("email");
        b.Property(x => x.EnviadoEm).HasColumnName("enviado_em");
    }
}
