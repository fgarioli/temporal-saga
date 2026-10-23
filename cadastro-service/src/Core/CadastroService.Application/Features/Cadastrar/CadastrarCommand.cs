namespace CadastroService.Application.Features.Cadastrar;

/// <summary>O pedido de cadastro, já validado e normalizado por quem chama (a API).</summary>
public sealed record CadastrarCommand(string Username, string Email, string Nome);
