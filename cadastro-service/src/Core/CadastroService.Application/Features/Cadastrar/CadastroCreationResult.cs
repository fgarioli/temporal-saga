using CadastroService.Core.Domain.Cadastros;

namespace CadastroService.Core.Application.Features.Cadastrar;

/// <summary>O desfecho de registrar um cadastro: criado (com o andamento inicial) ou recusado porque o username já está em uso.</summary>
public abstract record CadastroCreationResult
{
    public sealed record Created(CadastroState State) : CadastroCreationResult;

    public sealed record UsernameEmUso(string Mensagem) : CadastroCreationResult;
}
