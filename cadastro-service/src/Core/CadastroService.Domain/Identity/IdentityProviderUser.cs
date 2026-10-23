namespace CadastroService.Core.Domain.Identity;

/// <summary>Um usuário que já existe no provedor de identidade.</summary>
public sealed record IdentityProviderUser(string Id, string Username, string? Email);
