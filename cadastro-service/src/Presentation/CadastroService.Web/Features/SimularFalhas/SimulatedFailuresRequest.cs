namespace CadastroService.Presentation.Web.Features.SimularFalhas;

/// <summary>Quantas chamadas ao alvo (keycloak-create ou keycloak-enable) devem falhar; sem alvo, vale keycloak-create.</summary>
public sealed record SimulatedFailuresRequest(int Quantity, string? Alvo = null);
