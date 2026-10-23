using CadastroService.Core.Domain.Common;

namespace CadastroService.Core.Domain.Usuarios;

/// <summary>
/// O usuário da aplicação. O status também é o estado da saga do lado do banco: Pendente enquanto o cadastro
/// roda (é a trava semântica: não pode ser excluído), Ativo quando terminou, Rejeitado quando foi compensado.
/// </summary>
public sealed class Usuario
{
    public string Id { get; }
    public string Username { get; }
    public string Email { get; }
    public string Nome { get; }
    public StatusUsuario Status { get; private set; }
    public string? KeycloakId { get; private set; }
    public string? Motivo { get; private set; }

    // O EF Core usa este construtor para materializar o usuário (os parâmetros casam com as propriedades pelo nome).
    private Usuario(string id, string username, string email, string nome, StatusUsuario status, string? keycloakId, string? motivo)
    {
        Id = id;
        Username = username;
        Email = email;
        Nome = nome;
        Status = status;
        KeycloakId = keycloakId;
        Motivo = motivo;
    }

    /// <summary>T1: o usuário nasce pendente e ainda não pode fazer login.</summary>
    public static Usuario Create(string id, string username, string email, string nome) =>
        new(id, username, email, nome, StatusUsuario.Pendente, null, null);

    /// <summary>T4, repetível: grava o id do Keycloak e libera a trava.</summary>
    public void Ativar(string keycloakId)
    {
        if (Status == StatusUsuario.Ativo) return;
        if (Status == StatusUsuario.Rejeitado) throw new DomainException($"Usuário {Id} foi rejeitado e não pode ser ativado");
        Status = StatusUsuario.Ativo;
        KeycloakId = keycloakId;
    }

    /// <summary>C1, a compensação de T1: o cadastro termina rejeitado, com o motivo.</summary>
    public void Rejeitar(string motivo)
    {
        if (Status == StatusUsuario.Rejeitado) return;
        if (Status == StatusUsuario.Ativo) throw new DomainException($"Usuário {Id} já está ativo e não pode ser rejeitado");
        Status = StatusUsuario.Rejeitado;
        Motivo = motivo;
    }

    /// <summary>Semantic lock: enquanto a saga roda, a exclusão é recusada.</summary>
    public void GarantirQuePodeSerExcluido()
    {
        if (Status == StatusUsuario.Pendente)
            throw new DomainException($"O cadastro do usuário {Id} ainda está em andamento; tente de novo em instantes");
    }
}
