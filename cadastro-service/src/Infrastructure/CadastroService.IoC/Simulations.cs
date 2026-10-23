using Microsoft.Extensions.Configuration;

namespace CadastroService.IoC;

public static class Simulations
{
    public static bool Enabled(IConfiguration config) =>
        string.Equals(config["Simulations:Enabled"], "true", StringComparison.OrdinalIgnoreCase);
}
