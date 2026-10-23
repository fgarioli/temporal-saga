namespace CadastroService.Worker.Features.ProcessarCadastro.Temporal;

/// <summary>Os nomes que ligam quem inicia o workflow (o TemporalSagaRunner) a quem o define (o TemporalCadastroWorkflow) e ao worker.</summary>
public static class Saga
{
    public const string Workflow = "CadastroWorkflow";
    public const string TaskQueue = "cadastro";
}
