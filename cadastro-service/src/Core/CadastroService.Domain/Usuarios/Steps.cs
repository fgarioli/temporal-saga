namespace CadastroService.Core.Domain.Usuarios;

/// <summary>As etapas por que um usuário passa, do POST até o fim da saga.</summary>
public static class Steps
{
    public const string Pendente = "Pendente";
    public const string Iniciado = "Iniciado";
    public const string CriandoIdentidade = "CriandoIdentidade";
    public const string HabilitandoUsuario = "HabilitandoUsuario";
    public const string LiberandoAcesso = "LiberandoAcesso";
    public const string EnviandoBoasVindas = "EnviandoBoasVindas";
    public const string Concluido = "Concluido";
    public const string RejeitandoUsuario = "RejeitandoUsuario";
    public const string Cancelado = "Cancelado";
}
