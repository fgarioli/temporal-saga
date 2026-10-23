namespace CadastroService.Core.Domain.Envios;

/// <summary>Registro de que as boas-vindas de um cadastro já foram enviadas.</summary>
public sealed record Envio(string CadastroId, string Email, DateTimeOffset EnviadoEm);
