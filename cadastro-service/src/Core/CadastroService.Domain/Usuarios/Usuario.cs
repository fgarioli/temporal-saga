using CadastroService.Core.Domain.Common;

namespace CadastroService.Core.Domain.Usuarios;

/// <summary>
/// O usuário da aplicação e o andamento da saga que o provisiona. Nasce Pendente e desabilitado; vira Iniciado quando o dispatcher o
/// entrega ao motor e, a cada passo da saga, a etapa muda e entra no histórico. Habilitado só depois que a saga passa do pivô.
/// Rejeitado é desabilitado, na etapa Cancelado, com o motivo. O id é o id do usuário e o id do workflow.
/// </summary>
public sealed class Usuario
{
    public string Id { get; }
    public string Username { get; }
    public string Email { get; }
    public string Nome { get; }
    public bool Habilitado { get; private set; }
    public string Etapa { get; private set; }

    /// <summary>As etapas por que o usuário passou, em ordem. Iniciado não entra: é só a entrega ao motor.</summary>
    public IReadOnlyList<string> Historico { get; private set; } = [];

    public string? Motivo { get; private set; }
    public string? KeycloakId { get; private set; }
    public DateTimeOffset CriadoEm { get; }

    // O EF Core usa este construtor para materializar o usuário (os parâmetros casam com as propriedades pelo nome).
    private Usuario(string id, string username, string email, string nome, bool habilitado, string etapa, string? motivo, string? keycloakId, DateTimeOffset criadoEm)
    {
        Id = id;
        Username = username;
        Email = email;
        Nome = nome;
        Habilitado = habilitado;
        Etapa = etapa;
        Motivo = motivo;
        KeycloakId = keycloakId;
        CriadoEm = criadoEm;
    }

    /// <summary>O usuário nasce no POST: Pendente, desabilitado e com o histórico vazio.</summary>
    public static Usuario Create(string id, string username, string email, string nome, DateTimeOffset criadoEm) =>
        new(id, username, email, nome, false, Steps.Pendente, null, null, criadoEm);

    /// <summary>A saga foi entregue ao motor. Não entra no histórico.</summary>
    public void MarcarIniciado() => Etapa = Steps.Iniciado;

    /// <summary>
    /// Grava a etapa e a acrescenta ao histórico, a menos que ela já seja a última (repetir o mesmo passo não duplica).
    /// Motivo e keycloakId nulos mantêm o que já está gravado.
    /// </summary>
    public void RegistrarPasso(string etapa, string? motivo = null, string? keycloakId = null)
    {
        if (Historico.Count == 0 || Historico[^1] != etapa) Historico = [.. Historico, etapa];
        Etapa = etapa;
        Motivo = motivo ?? Motivo;
        KeycloakId = keycloakId ?? KeycloakId;
    }

    /// <summary>T3, repetível: o usuário passa a ser habilitado. Um usuário rejeitado não pode ser habilitado.</summary>
    public void Habilitar()
    {
        if (Habilitado) return;
        if (Etapa is Steps.RejeitandoUsuario or Steps.Cancelado) throw new DomainException($"Usuário {Id} foi rejeitado e não pode ser habilitado");
        Habilitado = true;
    }

    /// <summary>C1, repetível: a compensação do usuário criado no POST. Mantém o usuário desabilitado e grava o motivo.</summary>
    public void Rejeitar(string motivo)
    {
        if (Habilitado) throw new DomainException($"Usuário {Id} já está habilitado e não pode ser rejeitado");
        Motivo = motivo;
    }

    /// <summary>Semantic lock: enquanto a saga roda (etapa fora de Concluido e Cancelado), a exclusão é recusada.</summary>
    public void GarantirQuePodeSerExcluido()
    {
        if (Etapa is not (Steps.Concluido or Steps.Cancelado))
            throw new DomainException($"O provisionamento do usuário {Id} ainda está em andamento; tente de novo em instantes");
    }

    /// <summary>A entrada da saga.</summary>
    public UsuarioData ToData() => new(Id, Username, Email, Nome);
}
