namespace CadastroService.Worker.Features.ProcessarCadastro.Temporal;

/// <summary>Os nomes das activities e o tipo de erro que significa recusa de negócio.</summary>
public static class Activities
{
    public const string CriarUsuario = "CriarUsuario";
    public const string CriarNoKeycloak = "CriarNoKeycloak";
    public const string AtivarUsuario = "AtivarUsuario";
    public const string EnviarBoasVindas = "EnviarBoasVindas";
    public const string RejeitarUsuario = "RejeitarUsuario";
    public const string RegistrarPasso = "RegistrarPasso";
    public const string Refused = "Refused";
}
