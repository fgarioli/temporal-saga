using CadastroService.Application.Common.Cadastros;

namespace CadastroService.Application.Features.ProcessarCadastro;

/// <summary>
/// A saga de cadastro: T1 CriarUsuario (compensável), T2 CriarNoKeycloak (pivô), T3 AtivarUsuario, T4 EnviarBoasVindas;
/// a compensação C1 é RejeitarUsuario. Só orquestra: não conhece o motor que a executa.
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
        var compensable = false;
        try
        {
            // T1, compensável: o usuário nasce pendente no banco.
            await steps.RegistrarPasso(c.CadastroId, Steps.CriandoUsuario);
            await steps.CriarUsuario(c);
            compensable = true;

            // T2, o pivô: se der certo, o cadastro vai até o fim.
            await steps.RegistrarPasso(c.CadastroId, Steps.CriandoIdentidade);
            var keycloakId = await steps.CriarNoKeycloak(c);
            compensable = false;

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
            if (compensable)
            {
                // C1, a compensação de T1.
                await steps.RegistrarPasso(c.CadastroId, Steps.RejeitandoUsuario, motivo: refusal.Message);
                await steps.RejeitarUsuario(c.CadastroId, refusal.Message);
            }
            await steps.RegistrarPasso(c.CadastroId, Steps.Cancelado, motivo: refusal.Message);
            return Steps.Cancelado;
        }
    }
}
