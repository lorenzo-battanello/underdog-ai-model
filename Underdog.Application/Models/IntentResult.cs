namespace Underdog.Application.Models;

public sealed class IntentResult
{
    public required ParsedIntent ParsedObject { get; init; }

    public required string HumanPhrase { get; init; }

    public bool FromCache { get; init; }
}
