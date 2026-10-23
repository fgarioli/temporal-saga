namespace CadastroService.Infrastructure.Persistence.Entities;

/// <summary>Linha da tabela cadastros: o pedido de cadastro e o andamento da saga. O id é o id do cadastro e o id do workflow.</summary>
public sealed class CadastroEntity
{
    public string Id { get; set; } = "";
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string Nome { get; set; } = "";
    public string Estado { get; set; } = "";

    /// <summary>Lista de estados em JSON (coluna jsonb); o repositório converte de e para List de string.</summary>
    public string Historico { get; set; } = "[]";

    public string? Motivo { get; set; }
    public string? KeycloakId { get; set; }
    public DateTimeOffset CriadoEm { get; set; }
}
