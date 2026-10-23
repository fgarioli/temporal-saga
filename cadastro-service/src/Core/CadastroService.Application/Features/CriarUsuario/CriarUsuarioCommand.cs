namespace CadastroService.Core.Application.Features.CriarUsuario;

/// <summary>O pedido de criação do usuário, já validado e normalizado por quem chama (a API).</summary>
public sealed record CriarUsuarioCommand(string Username, string Email, string Nome);
