using CadastroService.Core.Domain.Cadastros;
using CadastroService.Core.Domain.Common;
using CadastroService.Core.Domain.Usuarios;

namespace CadastroService.Core.Application.Features.Cadastrar;

/// <summary>
/// Registra o pedido de cadastro. O usuário nasce Pendente aqui, de forma síncrona, junto com o cadastro Pendente, na mesma
/// transação. A API não executa a saga: o dispatcher entrega o cadastro ao motor, que começa pelo Keycloak.
/// </summary>
public sealed class CadastrarHandler(ICadastroRepository cadastros, IUsuarioRepository usuarios, IUnitOfWork unitOfWork, ILogger<CadastrarHandler> logger)
{
    public async Task<CadastroCreationResult> Handle(CadastrarCommand command, CancellationToken ct)
    {
        var id = Guid.NewGuid().ToString();
        if (await usuarios.IsUsernameInUse(command.Username, id, ct)) return UsernameEmUso(command.Username);

        var cadastro = Cadastro.Create(id, command.Username, command.Email, command.Nome, DateTimeOffset.UtcNow);
        cadastros.Add(cadastro);
        usuarios.Add(Usuario.Create(id, command.Username, command.Email, command.Nome));
        try
        {
            await unitOfWork.SaveChanges(ct);
        }
        catch (UniqueConstraintViolationException)
        {
            // Outro cadastro com o mesmo username gravou entre a checagem e o commit: o índice único decide.
            return UsernameEmUso(command.Username);
        }
        logger.LogInformation("Cadastro {Id} registrado para {Username}", id, command.Username);
        return new CadastroCreationResult.Created(cadastro.ToState());
    }

    private static CadastroCreationResult.UsernameEmUso UsernameEmUso(string username) => new($"O username {username} já está em uso");
}
