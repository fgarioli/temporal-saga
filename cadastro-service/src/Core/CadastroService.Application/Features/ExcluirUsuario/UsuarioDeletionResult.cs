namespace CadastroService.Application.Features.ExcluirUsuario;

public enum UsuarioDeletionStatus { Deleted, NotFound, CadastroEmAndamento, Conflict }

/// <summary>O resultado de excluir um usuário; Mensagem só vem preenchida quando a regra de negócio recusa.</summary>
public sealed record UsuarioDeletionResult(UsuarioDeletionStatus Status, string? Mensagem = null);
