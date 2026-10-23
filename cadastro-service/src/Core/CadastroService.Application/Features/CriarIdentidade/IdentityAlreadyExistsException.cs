namespace CadastroService.Core.Application.Features.CriarIdentidade;

/// <summary>O provedor de identidade já tem um usuário com esse username ou e-mail: recusa definitiva.</summary>
public sealed class IdentityAlreadyExistsException() : Exception("O username ou o e-mail já existe no Keycloak");
