namespace CadastroService.Infrastructure.Persistence.Entities;

public sealed class EnvioEntity
{
    public string CadastroId { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTimeOffset EnviadoEm { get; set; }
}
