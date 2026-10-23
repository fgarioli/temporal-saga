namespace CadastroService.Core.Domain.Usuarios;

/// <summary>Os dados do usuário que a saga recebe ao começar.</summary>
public sealed record UsuarioData(string UsuarioId, string Username, string Email, string Nome);
