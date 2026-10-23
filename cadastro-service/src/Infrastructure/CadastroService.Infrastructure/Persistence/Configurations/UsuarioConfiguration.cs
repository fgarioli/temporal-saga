using System.Text.Json;
using CadastroService.Core.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CadastroService.Infrastructure.Persistence.Configurations;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    // O histórico é a lista de etapas gravada como JSON na coluna jsonb.
    private static readonly ValueConverter<IReadOnlyList<string>, string> HistoricoConverter = new(
        historico => JsonSerializer.Serialize(historico, (JsonSerializerOptions?)null),
        json => JsonSerializer.Deserialize<List<string>>(json, (JsonSerializerOptions?)null) ?? new List<string>());

    // Sem o comparer, o EF não perceberia que a lista mudou: ele compara o conteúdo, não a referência.
    private static readonly ValueComparer<IReadOnlyList<string>> HistoricoComparer = new(
        (a, b) => a!.SequenceEqual(b!),
        historico => historico.Aggregate(0, (hash, etapa) => HashCode.Combine(hash, etapa.GetHashCode())),
        historico => historico.ToList());

    public void Configure(EntityTypeBuilder<Usuario> b)
    {
        b.ToTable("usuarios");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.Username).HasColumnName("username");
        b.Property(x => x.Email).HasColumnName("email");
        b.Property(x => x.Nome).HasColumnName("nome");
        b.Property(x => x.Habilitado).HasColumnName("habilitado");
        b.Property(x => x.Etapa).HasColumnName("etapa");
        b.Property(x => x.Historico).HasColumnName("historico").HasColumnType("jsonb").HasConversion(HistoricoConverter, HistoricoComparer);
        b.Property(x => x.Motivo).HasColumnName("motivo");
        b.Property(x => x.KeycloakId).HasColumnName("keycloak_id");
        b.Property(x => x.CriadoEm).HasColumnName("criado_em");
        // Concorrência otimista: o Postgres muda o xmin a cada atualização da linha. É uma propriedade sombra: o domínio não a conhece.
        b.Property<uint>("xmin").HasColumnName("xmin").HasColumnType("xid").ValueGeneratedOnAddOrUpdate().IsConcurrencyToken();
        // O dispatcher busca os pendentes por etapa.
        b.HasIndex(x => x.Etapa);
        // Um username só pode estar em uso por um usuário cuja saga não foi cancelada.
        b.HasIndex(x => x.Username).IsUnique().HasFilter($"etapa <> '{Steps.Cancelado}'");
    }
}
