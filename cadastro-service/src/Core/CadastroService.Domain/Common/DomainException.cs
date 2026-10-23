namespace CadastroService.Core.Domain.Common;

/// <summary>Regra de negócio violada: vira 409 na API.</summary>
public sealed class DomainException(string message) : Exception(message);
