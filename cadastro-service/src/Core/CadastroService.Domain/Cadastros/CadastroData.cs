namespace CadastroService.Core.Domain.Cadastros;

/// <summary>Os dados do pedido de cadastro: o que a saga recebe ao começar.</summary>
public sealed record CadastroData(string CadastroId, string Username, string Email, string Nome);
