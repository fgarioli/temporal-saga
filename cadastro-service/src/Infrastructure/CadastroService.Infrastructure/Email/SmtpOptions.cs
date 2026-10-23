namespace CadastroService.Infrastructure.Email;

public sealed class SmtpOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1025;
    public string Sender { get; set; } = "cadastro@saga.local";
}
