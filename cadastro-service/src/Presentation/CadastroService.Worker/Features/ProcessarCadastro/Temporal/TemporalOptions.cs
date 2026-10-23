namespace CadastroService.Presentation.Worker.Features.ProcessarCadastro.Temporal;

public sealed class TemporalOptions
{
    public string Address { get; set; } = "localhost:7233";
    public string Namespace { get; set; } = "default";
}
