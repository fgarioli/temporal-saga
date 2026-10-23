using CadastroService.Application.Common.Cadastros;
using CadastroService.Application.Common.Ports;

namespace CadastroService.Application.Features.ConsultarCadastro;

/// <summary>Consulta o andamento de um cadastro. Devolve null se ele não existe.</summary>
public sealed class ConsultarCadastroHandler(ICadastroRepository cadastros)
{
    public Task<CadastroState?> Handle(string cadastroId, CancellationToken ct) => cadastros.Get(cadastroId, ct);
}
