namespace CadastroService.Infrastructure.Persistence.Entities;

/// <summary>Linha da tabela usuarios. O domínio (Usuario) não conhece esta classe.</summary>
public sealed class UsuarioEntity
{
    public string Id { get; set; } = "";
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string Nome { get; set; } = "";
    public string Status { get; set; } = "";
    public string? KeycloakId { get; set; }
    public string? Motivo { get; set; }

    /// <summary>Concorrência otimista: o Postgres muda o xmin a cada atualização da linha.</summary>
    public uint Version { get; set; }
}
