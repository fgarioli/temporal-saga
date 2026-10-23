namespace CadastroService.Application.Features.ProcessarCadastro;

/// <summary>
/// Quanto tempo um passo pode levar e como repetir quando falha por motivo técnico: espera inicial, multiplicador e espera máxima
/// entre as tentativas. Não há limite de tentativas; só uma recusa de negócio interrompe. Quem executa a saga (o adaptador do motor)
/// converte estes valores para a configuração do motor.
/// </summary>
public sealed record StepPolicy(TimeSpan Timeout, TimeSpan InitialBackoff, float BackoffMultiplier, TimeSpan MaxBackoff)
{
    public static readonly StepPolicy Default = new(
        Timeout: TimeSpan.FromSeconds(30),
        InitialBackoff: TimeSpan.FromMilliseconds(200),
        BackoffMultiplier: 2,
        MaxBackoff: TimeSpan.FromSeconds(10));
}
