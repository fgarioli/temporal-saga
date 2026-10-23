using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CadastroService.Infrastructure.Persistence;

/// <summary>Usado só pela ferramenta dotnet ef para gerar as migrations.</summary>
public sealed class DesignTimeFactory : IDesignTimeDbContextFactory<CadastroDbContext>
{
    public CadastroDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<CadastroDbContext>().UseNpgsql("Host=localhost;Database=cadastro").Options);
}
