using System.Text.Json;
using CadastroService.Core.Domain.Cadastros;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CadastroService.Infrastructure.Persistence.Configurations;

public sealed class CadastroConfiguration : IEntityTypeConfiguration<Cadastro>
{
    // O histórico é a lista de estados gravada como JSON na coluna jsonb.
    private static readonly ValueConverter<IReadOnlyList<string>, string> HistoricoConverter = new(
        historico => JsonSerializer.Serialize(historico, (JsonSerializerOptions?)null),
        json => JsonSerializer.Deserialize<List<string>>(json, (JsonSerializerOptions?)null) ?? new List<string>());

    // Sem o comparer, o EF não perceberia que a lista mudou: ele compara o conteúdo, não a referência.
    private static readonly ValueComparer<IReadOnlyList<string>> HistoricoComparer = new(
        (a, b) => a!.SequenceEqual(b!),
        historico => historico.Aggregate(0, (hash, estado) => HashCode.Combine(hash, estado.GetHashCode())),
        historico => historico.ToList());

    public void Configure(EntityTypeBuilder<Cadastro> b)
    {
        b.ToTable("cadastros");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.Username).HasColumnName("username");
        b.Property(x => x.Email).HasColumnName("email");
        b.Property(x => x.Nome).HasColumnName("nome");
        b.Property(x => x.Estado).HasColumnName("estado");
        b.Property(x => x.Historico).HasColumnName("historico").HasColumnType("jsonb").HasConversion(HistoricoConverter, HistoricoComparer);
        b.Property(x => x.Motivo).HasColumnName("motivo");
        b.Property(x => x.KeycloakId).HasColumnName("keycloak_id");
        b.Property(x => x.CriadoEm).HasColumnName("criado_em");
        // O dispatcher busca os pendentes por estado.
        b.HasIndex(x => x.Estado);
    }
}
