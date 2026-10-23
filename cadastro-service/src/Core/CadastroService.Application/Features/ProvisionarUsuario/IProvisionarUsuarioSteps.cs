using CadastroService.Core.Domain.Usuarios;

namespace CadastroService.Core.Application.Features.ProvisionarUsuario;

/// <summary>
/// Os passos da saga de provisionamento, como a saga os enxerga: cada método é um passo que o motor executa (e repete, se falhar por
/// motivo técnico). Quem implementa decide como: o adaptador do motor chama uma tarefa dele; um teste usa um fake.
/// Uma recusa de negócio sobe como <see cref="StepRefusedException"/>.
/// </summary>
public interface IProvisionarUsuarioSteps
{
    /// <summary>T2, o pivô: cria a identidade no Keycloak, desabilitada, e devolve o id dela.</summary>
    Task<string> CriarNoKeycloak(UsuarioData data);

    /// <summary>T3, repetível: habilita o usuário no banco.</summary>
    Task HabilitarUsuario(string usuarioId);

    /// <summary>T4, repetível: habilita a identidade no Keycloak.</summary>
    Task LiberarAcesso(string keycloakId);

    /// <summary>C1, a compensação do usuário criado no POST.</summary>
    Task RejeitarUsuario(string usuarioId, string motivo);

    /// <summary>T5, repetível.</summary>
    Task EnviarBoasVindas(UsuarioData data);

    /// <summary>Grava a etapa em que o usuário está agora e a acrescenta ao histórico. Motivo e keycloakId nulos mantêm o que já está gravado.</summary>
    Task RegistrarPasso(string usuarioId, string etapa, string? motivo = null, string? keycloakId = null);
}
