using CadastroService.Application.Common.Ports;

namespace CadastroService.Application.Features.ProcessarCadastro;

/// <summary>Grava o andamento do cadastro (estado, histórico, motivo e id no Keycloak) que a consulta da API devolve.</summary>
public sealed class RegistrarPassoHandler(ICadastroRepository cadastros)
{
    public Task Handle(string cadastroId, string estado, string? motivo, string? keycloakId, CancellationToken ct) =>
        cadastros.RegistrarPasso(cadastroId, estado, motivo, keycloakId, ct);
}
