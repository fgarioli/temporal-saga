namespace CadastroService.Core.Application.Common.Cadastros;

/// <summary>O andamento de um cadastro: o que a consulta da API devolve.</summary>
public sealed record CadastroState(string CadastroId, string Username, string Email, string Estado,
    IReadOnlyList<string> Historico, string? Motivo, string? KeycloakId);
