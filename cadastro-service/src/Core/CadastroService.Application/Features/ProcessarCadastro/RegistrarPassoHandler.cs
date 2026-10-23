using CadastroService.Core.Domain.Cadastros;

namespace CadastroService.Core.Application.Features.ProcessarCadastro;

/// <summary>
/// Grava o andamento do cadastro (estado, histórico, motivo e id no Keycloak) que a consulta da API devolve.
/// Não grava nada: quem chamou (a activity) faz o commit.
/// </summary>
public sealed class RegistrarPassoHandler(ICadastroRepository cadastros)
{
    public async Task Handle(string cadastroId, string estado, string? motivo, string? keycloakId, CancellationToken ct)
    {
        var cadastro = await cadastros.Get(cadastroId, ct) ?? throw new InvalidOperationException($"Cadastro {cadastroId} não encontrado");
        cadastro.RegistrarPasso(estado, motivo, keycloakId);
    }
}
