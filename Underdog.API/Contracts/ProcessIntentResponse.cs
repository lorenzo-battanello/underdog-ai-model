namespace Underdog.API.Contracts;

public sealed class ProcessIntentResponse
{
    public required object ParsedObject { get; init; }

    public required string HumanPhrase { get; init; }

    public bool FromCache { get; init; }
}
