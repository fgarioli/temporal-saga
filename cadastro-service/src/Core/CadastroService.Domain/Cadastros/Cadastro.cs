namespace CadastroService.Core.Domain.Cadastros;

/// <summary>
/// O pedido de cadastro e o andamento da saga: nasce Pendente, vira Iniciado quando o dispatcher o entrega ao motor e, a cada passo
/// da saga, o estado muda e entra no histórico. O id é o id do cadastro e o id do workflow.
/// </summary>
public sealed class Cadastro
{
    public string Id { get; }
    public string Username { get; }
    public string Email { get; }
    public string Nome { get; }
    public string Estado { get; private set; }

    /// <summary>Os estados por que o cadastro passou, em ordem. Iniciado não entra: é só a entrega ao motor.</summary>
    public IReadOnlyList<string> Historico { get; private set; } = [];

    public string? Motivo { get; private set; }
    public string? KeycloakId { get; private set; }
    public DateTimeOffset CriadoEm { get; }

    private Cadastro(string id, string username, string email, string nome, string estado, string? motivo, string? keycloakId, DateTimeOffset criadoEm)
    {
        Id = id;
        Username = username;
        Email = email;
        Nome = nome;
        Estado = estado;
        Motivo = motivo;
        KeycloakId = keycloakId;
        CriadoEm = criadoEm;
    }

    /// <summary>O pedido nasce Pendente, com o histórico vazio.</summary>
    public static Cadastro Create(string id, string username, string email, string nome, DateTimeOffset criadoEm) =>
        new(id, username, email, nome, Steps.Pendente, null, null, criadoEm);

    /// <summary>A saga foi entregue ao motor. Não entra no histórico.</summary>
    public void MarcarIniciado() => Estado = Steps.Iniciado;

    /// <summary>
    /// Grava o estado e o acrescenta ao histórico, a menos que ele já seja o último (repetir o mesmo passo não duplica).
    /// Motivo e keycloakId nulos mantêm o que já está gravado.
    /// </summary>
    public void RegistrarPasso(string estado, string? motivo = null, string? keycloakId = null)
    {
        if (Historico.Count == 0 || Historico[^1] != estado) Historico = [.. Historico, estado];
        Estado = estado;
        Motivo = motivo ?? Motivo;
        KeycloakId = keycloakId ?? KeycloakId;
    }

    /// <summary>A entrada da saga.</summary>
    public CadastroData ToData() => new(Id, Username, Email, Nome);

    /// <summary>O andamento que a consulta da API devolve.</summary>
    public CadastroState ToState() => new(Id, Username, Email, Estado, Historico, Motivo, KeycloakId);
}
