using CadastroService.Application.Common.Cadastros;
using CadastroService.Application.Common.Ports;
using CadastroService.Domain.Common;

namespace CadastroService.Application.Features.Cadastrar;

/// <summary>
/// Registra o pedido de cadastro. A API não executa a saga: ela só grava o pedido como Pendente, e o dispatcher o entrega ao motor.
/// </summary>
public sealed class CadastrarHandler(ICadastroRepository cadastros, IUnitOfWork unitOfWork, ILogger<CadastrarHandler> logger)
{
    public async Task<CadastroState> Handle(CadastrarCommand command, CancellationToken ct)
    {
        var data = new CadastroData(Guid.NewGuid().ToString(), command.Username, command.Email, command.Nome);
        cadastros.Add(data);
        await unitOfWork.SaveChanges(ct);
        logger.LogInformation("Cadastro {Id} registrado para {Username}", data.CadastroId, data.Username);
        return new CadastroState(data.CadastroId, data.Username, data.Email, Steps.Pendente, [], null, null);
    }
}
