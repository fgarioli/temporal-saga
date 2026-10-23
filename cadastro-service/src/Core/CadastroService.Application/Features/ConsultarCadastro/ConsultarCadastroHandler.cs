using CadastroService.Core.Domain.Cadastros;

namespace CadastroService.Core.Application.Features.ConsultarCadastro;

/// <summary>Consulta o andamento de um cadastro. Devolve null se ele não existe.</summary>
public sealed class ConsultarCadastroHandler(ICadastroRepository cadastros)
{
    public async Task<CadastroState?> Handle(string cadastroId, CancellationToken ct) =>
        (await cadastros.Get(cadastroId, ct))?.ToState();
}
