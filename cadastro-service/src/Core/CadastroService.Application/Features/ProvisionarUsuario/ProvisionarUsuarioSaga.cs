using CadastroService.Core.Domain.Usuarios;

namespace CadastroService.Core.Application.Features.ProvisionarUsuario;

/// <summary>
/// A saga que provisiona o usuário: T2 CriarNoKeycloak (pivô, a identidade nasce desabilitada), T3 HabilitarUsuario, T4 LiberarAcesso
/// (habilita a identidade no Keycloak), T5 EnviarBoasVindas; a compensação C1 é RejeitarUsuario.
/// O passo T1, criar o usuário Pendente, não é da saga: o POST /usuarios o executa de forma síncrona, antes de a saga começar.
/// Só orquestra: não conhece o motor que a executa.
/// <para>
/// Regra de determinismo: o motor pode reexecutar (replay) este código para reconstruir o estado depois de uma queda. Por isso
/// aqui não entram DateTime.Now, Guid.NewGuid, Task.Run, Task.Delay, I/O direto nem ConfigureAwait(false). Tudo o que muda o
/// mundo passa por <see cref="IProvisionarUsuarioSteps"/>.
/// </para>
/// </summary>
public sealed class ProvisionarUsuarioSaga(IProvisionarUsuarioSteps steps)
{
    /// <summary>Executa a saga até o fim e devolve a etapa final: Concluido ou Cancelado.</summary>
    public async Task<string> Run(UsuarioData u)
    {
        try
        {
            // T2, o pivô: se der certo, o usuário vai até o fim.
            await steps.RegistrarPasso(u.UsuarioId, Steps.CriandoIdentidade);
            var keycloakId = await steps.CriarNoKeycloak(u);

            // T3, T4 e T5, repetíveis: não falham por regra de negócio, só por erro técnico.
            await steps.RegistrarPasso(u.UsuarioId, Steps.HabilitandoUsuario, keycloakId: keycloakId);
            await steps.HabilitarUsuario(u.UsuarioId);

            await steps.RegistrarPasso(u.UsuarioId, Steps.LiberandoAcesso);
            await steps.LiberarAcesso(keycloakId);

            await steps.RegistrarPasso(u.UsuarioId, Steps.EnviandoBoasVindas);
            await steps.EnviarBoasVindas(u);

            await steps.RegistrarPasso(u.UsuarioId, Steps.Concluido);
            return Steps.Concluido;
        }
        catch (StepRefusedException refusal)
        {
            // C1, a compensação do usuário criado no POST.
            await steps.RegistrarPasso(u.UsuarioId, Steps.RejeitandoUsuario, motivo: refusal.Message);
            await steps.RejeitarUsuario(u.UsuarioId, refusal.Message);
            await steps.RegistrarPasso(u.UsuarioId, Steps.Cancelado, motivo: refusal.Message);
            return Steps.Cancelado;
        }
    }
}
