namespace CadastroService.Infrastructure.Identity;

public sealed class KeycloakOptions
{
    public string Url { get; set; } = "http://localhost:8180";
    public string Realm { get; set; } = "saga";
    public string Admin { get; set; } = "admin";
    public string Password { get; set; } = "admin";
}
