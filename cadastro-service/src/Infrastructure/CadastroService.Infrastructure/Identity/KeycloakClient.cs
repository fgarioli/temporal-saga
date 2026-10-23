using System.Net;
using System.Net.Http.Json;
using CadastroService.Core.Domain.Identity;
using Microsoft.Extensions.Options;

namespace CadastroService.Infrastructure.Identity;

/// <summary>
/// Adapter da Admin REST API do Keycloak. O perfil padrão do usuário não é responsabilidade deste serviço:
/// vem do default role do realm, que o Keycloak aplica sozinho a todo usuário novo.
/// </summary>
public sealed class KeycloakClient(HttpClient http, IOptions<KeycloakOptions> options) : IIdentityProvider
{
    private readonly KeycloakOptions kc = options.Value;

    /// <summary>Cria a identidade desabilitada: o usuário só consegue fazer login depois de <see cref="Enable"/>.</summary>
    public async Task<string> Create(NewIdentity newIdentity, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{kc.Url}/admin/realms/{kc.Realm}/users")
        {
            Content = JsonContent.Create(new
            {
                username = newIdentity.Username,
                email = newIdentity.Email,
                firstName = newIdentity.PrimeiroNome,
                lastName = newIdentity.Sobrenome,
                enabled = false,
                emailVerified = true,
            }),
        };
        request.Headers.Authorization = new("Bearer", await Token(ct));
        using var response = await http.SendAsync(request, ct);
        if (response.StatusCode == HttpStatusCode.Conflict) throw new IdentityAlreadyExistsException();
        response.EnsureSuccessStatusCode();
        var location = response.Headers.Location?.ToString() ?? throw new InvalidOperationException("O Keycloak não devolveu o Location");
        return location[(location.LastIndexOf('/') + 1)..];
    }

    /// <summary>PUT no usuário com enabled=true. Repetível: o Keycloak responde 204 também quando ele já está habilitado.</summary>
    public async Task Enable(string keycloakId, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, $"{kc.Url}/admin/realms/{kc.Realm}/users/{Uri.EscapeDataString(keycloakId)}")
        {
            Content = JsonContent.Create(new { enabled = true }),
        };
        request.Headers.Authorization = new("Bearer", await Token(ct));
        using var response = await http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IdentityProviderUser?> FindByUsername(string username, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get,
            $"{kc.Url}/admin/realms/{kc.Realm}/users?exact=true&username={Uri.EscapeDataString(username)}");
        request.Headers.Authorization = new("Bearer", await Token(ct));
        using var response = await http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        var users = await response.Content.ReadFromJsonAsync<List<IdentityProviderUser>>(ct) ?? [];
        return users.FirstOrDefault();
    }

    private async Task<string> Token(CancellationToken ct)
    {
        using var response = await http.PostAsync($"{kc.Url}/realms/master/protocol/openid-connect/token", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = "admin-cli",
                ["username"] = kc.Admin,
                ["password"] = kc.Password,
            }), ct);
        response.EnsureSuccessStatusCode();
        var token = await response.Content.ReadFromJsonAsync<TokenResponse>(ct);
        return token?.access_token ?? throw new InvalidOperationException("O Keycloak não devolveu o token");
    }

    private sealed record TokenResponse(string access_token);
}
