namespace Underdog.API.Contracts;

public sealed class ProcessIntentRequest
{
    public string UserPrompt { get; init; } = string.Empty;
}
