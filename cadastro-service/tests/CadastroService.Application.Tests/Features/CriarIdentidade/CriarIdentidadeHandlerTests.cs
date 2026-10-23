using CadastroService.Core.Application.Features.CriarIdentidade;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CadastroService.Core.Application.Tests.Features.CriarIdentidade;

public class CriarIdentidadeHandlerTests
{
    private sealed class FakeIdentityProvider : IIdentityProvider
    {
        public Dictionary<string, IdentityProviderUser> Existing { get; } = [];
        public int Calls { get; private set; }

        public Task<string> Create(NewIdentity newIdentity, CancellationToken ct)
        {
            Calls++;
            if (Existing.ContainsKey(newIdentity.Username)) throw new IdentityAlreadyExistsException();
            Existing[newIdentity.Username] = new IdentityProviderUser($"kc-{newIdentity.Username}", newIdentity.Username, newIdentity.Email);
            return Task.FromResult($"kc-{newIdentity.Username}");
        }

        public Task<IdentityProviderUser?> FindByUsername(string username, CancellationToken ct) =>
            Task.FromResult(Existing.GetValueOrDefault(username));
    }

    private static readonly NewIdentity Ana = new("ana", "ana@x.com", "Ana Souza");

    [Fact]
    public async Task CriaEDevolveOId()
    {
        var result = await new CriarIdentidadeHandler(new FakeIdentityProvider(), NullLogger<CriarIdentidadeHandler>.Instance).Handle(Ana, default);
        Assert.Equal("kc-ana", result.KeycloakId);
    }

    [Fact]
    public async Task UsuarioDeOutroCadastroERecusado()
    {
        var provider = new FakeIdentityProvider();
        provider.Existing["ana"] = new IdentityProviderUser("kc-x", "ana", "outra@x.com");
        var result = await new CriarIdentidadeHandler(provider, NullLogger<CriarIdentidadeHandler>.Instance).Handle(Ana, default);
        Assert.True(result.Refused);
    }

    [Fact]
    public async Task TentativaAnteriorDoMesmoCadastroEReconhecida()
    {
        var provider = new FakeIdentityProvider();
        provider.Existing["ana"] = new IdentityProviderUser("kc-ana", "ana", "ANA@x.com");
        var result = await new CriarIdentidadeHandler(provider, NullLogger<CriarIdentidadeHandler>.Instance).Handle(Ana, default);
        Assert.Equal("kc-ana", result.KeycloakId);
    }
}
