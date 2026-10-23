namespace CadastroService.Core.Application.Features.ProcessarCadastro;

/// <summary>
/// Um passo da saga recusou o cadastro por regra de negócio (por exemplo, username já em uso). Não adianta tentar de novo:
/// a saga compensa o que já fez e cancela. Falhas técnicas não usam esta exceção; o motor repete o passo.
/// </summary>
public sealed class StepRefusedException(string motivo) : Exception(motivo);
