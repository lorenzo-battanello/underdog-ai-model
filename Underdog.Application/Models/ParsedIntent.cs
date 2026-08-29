namespace Underdog.Application.Models;

public sealed class ParsedIntent
{
    public string Action { get; init; } = string.Empty;

    public string Subject { get; init; } = string.Empty;

    public string HumanPhrase { get; init; } = string.Empty;
}
