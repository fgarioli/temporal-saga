namespace CadastroService.Presentation.Worker.Features.ProvisionarUsuario.Temporal;

/// <summary>Os nomes que ligam quem inicia o workflow (o TemporalSagaRunner) a quem o define (o TemporalProvisionarUsuarioWorkflow) e ao worker.</summary>
public static class Saga
{
    public const string Workflow = "ProvisionarUsuarioWorkflow";
    public const string TaskQueue = "provisionamento";
}
