namespace CadastroService.Core.Application.Features.CriarIdentidade;

/// <summary>Um usuário que já existe no provedor de identidade.</summary>
public sealed record IdentityProviderUser(string Id, string Username, string? Email);
