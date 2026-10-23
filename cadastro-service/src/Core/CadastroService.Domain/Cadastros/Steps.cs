namespace CadastroService.Core.Domain.Cadastros;

/// <summary>Os estados por que um cadastro passa.</summary>
public static class Steps
{
    public const string Pendente = "Pendente";
    public const string Iniciado = "Iniciado";
    public const string CriandoIdentidade = "CriandoIdentidade";
    public const string AtivandoUsuario = "AtivandoUsuario";
    public const string EnviandoBoasVindas = "EnviandoBoasVindas";
    public const string Concluido = "Concluido";
    public const string RejeitandoUsuario = "RejeitandoUsuario";
    public const string Cancelado = "Cancelado";
}
