using System.Net.Mail;
using CadastroService.Core.Domain.Notifications;
using Microsoft.Extensions.Options;

namespace CadastroService.Infrastructure.Email;

public sealed class SmtpEmailSender(IOptions<SmtpOptions> options) : IEmailSender
{
    public async Task Send(string to, string subject, string body, CancellationToken ct)
    {
        using var client = new SmtpClient(options.Value.Host, options.Value.Port);
        using var message = new MailMessage(options.Value.Sender, to, subject, body);
        await client.SendMailAsync(message, ct);
    }
}
