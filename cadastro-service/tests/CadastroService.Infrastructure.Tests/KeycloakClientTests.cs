using System.Net;
using System.Text;
using System.Text.Json;
using CadastroService.Core.Domain.Identity;
using CadastroService.Infrastructure.Identity;
using Microsoft.Extensions.Options;
using Xunit;

namespace CadastroService.Infrastructure.Tests;

/// <summary>
/// Confere o que o KeycloakClient manda à Admin REST API, com um HttpMessageHandler falso: não precisa de Postgres nem de Keycloak.
/// Roda junto dos testes de integração: docker compose run --rm integration-tests.
/// </summary>
[Trait("Category", "Integration")]
public class KeycloakClientTests
{
    private sealed record Chamada(HttpMethod Method, string Url, string? Body);

    private sealed class FakeKeycloak : HttpMessageHandler
    {
        public List<Chamada> Chamadas { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(ct);
            var url = request.RequestUri!.ToString();
            Chamadas.Add(new Chamada(request.Method, url, body));
            if (url.EndsWith("/protocol/openid-connect/token"))
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""{"access_token":"t"}""", Encoding.UTF8, "application/json") };
            if (request.Method == HttpMethod.Post)
            {
                var created = new HttpResponseMessage(HttpStatusCode.Created);
                created.Headers.Location = new Uri($"{url}/kc-ana");
                return created;
            }
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }
    }

    private static (KeycloakClient Client, FakeKeycloak Keycloak) NewClient()
    {
        var keycloak = new FakeKeycloak();
        var options = Options.Create(new KeycloakOptions { Url = "http://kc", Realm = "saga" });
        return (new KeycloakClient(new HttpClient(keycloak), options), keycloak);
    }

    [Fact]
    public async Task CreateCriaAIdentidadeDesabilitada()
    {
        var (client, keycloak) = NewClient();

        var id = await client.Create(new NewIdentity("ana", "ana@x.com", "Ana Souza"), default);

        Assert.Equal("kc-ana", id);
        var post = Assert.Single(keycloak.Chamadas, c => c.Url == "http://kc/admin/realms/saga/users");
        using var json = JsonDocument.Parse(post.Body!);
        Assert.False(json.RootElement.GetProperty("enabled").GetBoolean());
        Assert.Equal("ana", json.RootElement.GetProperty("username").GetString());
    }

    [Fact]
    public async Task EnableHabilitaAIdentidadeComUmPutNoUsuario()
    {
        var (client, keycloak) = NewClient();

        await client.Enable("kc-ana", default);
        await client.Enable("kc-ana", default);

        var puts = keycloak.Chamadas.Where(c => c.Method == HttpMethod.Put).ToList();
        Assert.Equal(2, puts.Count);
        Assert.All(puts, c =>
        {
            Assert.Equal("http://kc/admin/realms/saga/users/kc-ana", c.Url);
            using var json = JsonDocument.Parse(c.Body!);
            Assert.True(json.RootElement.GetProperty("enabled").GetBoolean());
        });
    }
}
