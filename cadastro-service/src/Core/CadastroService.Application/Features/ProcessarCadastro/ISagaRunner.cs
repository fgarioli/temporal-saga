using CadastroService.Core.Application.Common.Cadastros;

namespace CadastroService.Core.Application.Features.ProcessarCadastro;

/// <summary>Porta de saída: quem entrega a saga ao motor que a executa (implementado no Worker).</summary>
public interface ISagaRunner
{
    /// <summary>Começa a saga do cadastro. Idempotente: o id do cadastro identifica a saga, e pedir de novo não a executa duas vezes.</summary>
    Task Start(CadastroData data, CancellationToken ct);
}
