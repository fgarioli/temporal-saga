namespace CadastroService.Core.Domain.Common;

/// <summary>
/// A gravação violou uma restrição de unicidade (por exemplo, dois cadastros disputando o mesmo username).
/// Independe do mecanismo de persistência; a causa original fica em InnerException.
/// </summary>
public sealed class UniqueConstraintViolationException(string message, Exception? innerException = null) : Exception(message, innerException);
