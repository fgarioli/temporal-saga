using CadastroService.Core.Domain.Notifications;

using CadastroService.Core.Domain.Envios;

namespace CadastroService.Core.Application.Features.EnviarBoasVindas;

/// <summary>
/// T5, repetível. O registro do envio evita mandar o e-mail de novo quando o Temporal repete a activity.
/// Se o SMTP falhar, nada é gravado e o Temporal tenta de novo.
/// </summary>
public sealed class EnviarBoasVindasHandler(IEnvioRepository envios, IEmailSender emailSender, ILogger<EnviarBoasVindasHandler> logger)
{
    public async Task Handle(string usuarioId, string to, string nome, CancellationToken ct)
    {
        if (await envios.WasSent(usuarioId, ct)) return;
        envios.Add(new Envio(usuarioId, to, DateTimeOffset.UtcNow));
        await emailSender.Send(to, "Bem-vindo!", $"Olá, {nome}! Seu acesso foi liberado.", ct);
        logger.LogInformation("Boas-vindas enviadas para {Email}", to);
    }
}
