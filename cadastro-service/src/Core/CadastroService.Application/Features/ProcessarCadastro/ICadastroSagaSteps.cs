using CadastroService.Core.Domain.Cadastros;

namespace CadastroService.Core.Application.Features.ProcessarCadastro;

/// <summary>
/// Os passos da saga de cadastro, como a saga os enxerga: cada método é um passo que o motor executa (e repete, se falhar por
/// motivo técnico). Quem implementa decide como: o adaptador do motor chama uma tarefa dele; um teste usa um fake.
/// Uma recusa de negócio sobe como <see cref="StepRefusedException"/>.
/// </summary>
public interface ICadastroSagaSteps
{
    /// <summary>T2, o pivô: cria a identidade no Keycloak e devolve o id dela.</summary>
    Task<string> CriarNoKeycloak(CadastroData data);

    /// <summary>T3, repetível.</summary>
    Task AtivarUsuario(string cadastroId, string keycloakId);

    /// <summary>C1, a compensação do usuário criado no POST.</summary>
    Task RejeitarUsuario(string cadastroId, string motivo);

    /// <summary>T4, repetível.</summary>
    Task EnviarBoasVindas(CadastroData data);

    /// <summary>Grava o estado em que o cadastro está agora e o acrescenta ao histórico. Motivo e keycloakId nulos mantêm o que já está gravado.</summary>
    Task RegistrarPasso(string cadastroId, string estado, string? motivo = null, string? keycloakId = null);
}
