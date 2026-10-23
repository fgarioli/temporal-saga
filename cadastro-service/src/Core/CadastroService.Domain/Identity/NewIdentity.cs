namespace CadastroService.Core.Domain.Identity;

/// <summary>O que o cadastro pede para criar no provedor de identidade.</summary>
public sealed record NewIdentity(string Username, string Email, string Nome)
{
    public string PrimeiroNome => Nome.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? Username;

    public string Sobrenome
    {
        get
        {
            var parts = Nome.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length < 2 ? PrimeiroNome : string.Join(' ', parts[1..]);
        }
    }
}
