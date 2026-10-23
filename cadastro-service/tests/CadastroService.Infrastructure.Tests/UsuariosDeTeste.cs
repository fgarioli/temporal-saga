using CadastroService.Core.Domain.Usuarios;

namespace CadastroService.Infrastructure.Tests;

internal static class UsuariosDeTeste
{
    /// <summary>
    /// Já Iniciado: o banco de teste é o mesmo do worker, e o dispatcher só entrega à saga o usuário Pendente.
    /// </summary>
    public static Usuario Novo(string id, string username, string email, string nome)
    {
        var usuario = Usuario.Create(id, username, email, nome, DateTimeOffset.UtcNow);
        usuario.MarcarIniciado();
        return usuario;
    }
}
