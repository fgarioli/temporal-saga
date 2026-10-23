namespace CadastroService.Application.Features.EnviarBoasVindas;

/// <summary>Porta de saída: quem entrega o e-mail.</summary>
public interface IEmailSender
{
    Task Send(string to, string subject, string body, CancellationToken ct);
}
