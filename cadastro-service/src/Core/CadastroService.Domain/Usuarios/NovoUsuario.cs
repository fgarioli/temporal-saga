using System.Text.RegularExpressions;

namespace CadastroService.Core.Domain.Usuarios;

/// <summary>Pedido de criação de usuário já validado e normalizado.</summary>
public sealed partial record NovoUsuario
{
    public string Username { get; }
    public string Email { get; }
    public string Nome { get; }

    private NovoUsuario(string username, string email, string nome)
    {
        Username = username;
        Email = email;
        Nome = nome;
    }

    /// <summary>Lança ArgumentException quando um campo é inválido.</summary>
    public static NovoUsuario Create(string? username, string? email, string? nome)
    {
        var u = (username ?? "").Trim().ToLowerInvariant();
        var e = (email ?? "").Trim().ToLowerInvariant();
        var n = (nome ?? "").Trim();
        if (!ValidUsernameRegex().IsMatch(u)) throw new ArgumentException("username deve ter de 3 a 30 letras, números, ponto, hífen ou sublinhado");
        if (!e.Contains('@')) throw new ArgumentException("email inválido");
        if (n.Length == 0) throw new ArgumentException("nome é obrigatório");
        return new NovoUsuario(u, e, n);
    }

    [GeneratedRegex("^[a-z0-9._-]{3,30}$")]
    private static partial Regex ValidUsernameRegex();
}
