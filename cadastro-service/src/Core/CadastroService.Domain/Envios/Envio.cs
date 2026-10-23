namespace CadastroService.Core.Domain.Envios;

/// <summary>Registro de que as boas-vindas de um usuário já foram enviadas.</summary>
public sealed record Envio(string UsuarioId, string Email, DateTimeOffset EnviadoEm);
