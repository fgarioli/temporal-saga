using CadastroService.Core.Domain.Usuarios;

namespace CadastroService.Core.Application.Features.ProvisionarUsuario;

/// <summary>Porta de saída: quem entrega a saga ao motor que a executa (implementado no Worker).</summary>
public interface ISagaRunner
{
    /// <summary>Começa a saga do usuário. Idempotente: o id do usuário identifica a saga, e pedir de novo não a executa duas vezes.</summary>
    Task Start(UsuarioData data, CancellationToken ct);
}
