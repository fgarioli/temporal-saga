namespace CadastroService.Domain.Common;

/// <summary>
/// Outra operação mudou o mesmo registro entre a leitura e a gravação (conflito de versão).
/// Independe do mecanismo de persistência; a causa original fica em InnerException.
/// </summary>
public sealed class ConcurrencyConflictException(string message, Exception? innerException = null) : Exception(message, innerException);
