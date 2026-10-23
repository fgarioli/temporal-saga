using CadastroService.Core.Domain.Cadastros;

namespace CadastroService.Core.Application.Features.ProcessarCadastro;

/// <summary>
/// A saga de cadastro: T2 CriarNoKeycloak (pivô), T3 AtivarUsuario, T4 EnviarBoasVindas; a compensação C1 é RejeitarUsuario.
/// O passo T1, criar o usuário Pendente, não é da saga: o POST /cadastros o executa de forma síncrona, antes de a saga começar.
/// Só orquestra: não conhece o motor que a executa.
/// <para>
/// Regra de determinismo: o motor pode reexecutar (replay) este código para reconstruir o estado depois de uma queda. Por isso
/// aqui não entram DateTime.Now, Guid.NewGuid, Task.Run, Task.Delay, I/O direto nem ConfigureAwait(false). Tudo o que muda o
/// mundo passa por <see cref="ICadastroSagaSteps"/>.
/// </para>
/// </summary>
public sealed class CadastroSaga(ICadastroSagaSteps steps)
{
    /// <summary>Executa a saga até o fim e devolve o estado final: Concluido ou Cancelado.</summary>
    public async Task<string> Run(CadastroData c)
    {
        try
        {
            // T2, o pivô: se der certo, o cadastro vai até o fim.
            await steps.RegistrarPasso(c.CadastroId, Steps.CriandoIdentidade);
            var keycloakId = await steps.CriarNoKeycloak(c);

            // T3 e T4, repetíveis: não falham por regra de negócio, só por erro técnico.
            await steps.RegistrarPasso(c.CadastroId, Steps.AtivandoUsuario, keycloakId: keycloakId);
            await steps.AtivarUsuario(c.CadastroId, keycloakId);

            await steps.RegistrarPasso(c.CadastroId, Steps.EnviandoBoasVindas);
            await steps.EnviarBoasVindas(c);

            await steps.RegistrarPasso(c.CadastroId, Steps.Concluido);
            return Steps.Concluido;
        }
        catch (StepRefusedException refusal)
        {
            // C1, a compensação do usuário criado no POST.
            await steps.RegistrarPasso(c.CadastroId, Steps.RejeitandoUsuario, motivo: refusal.Message);
            await steps.RejeitarUsuario(c.CadastroId, refusal.Message);
            await steps.RegistrarPasso(c.CadastroId, Steps.Cancelado, motivo: refusal.Message);
            return Steps.Cancelado;
        }
    }
}
