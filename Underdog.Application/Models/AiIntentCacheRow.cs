namespace Underdog.Application.Models;

public sealed class AiIntentCacheRow
{
    public ulong Id { get; set; }

    public string NormalizedPromptHash { get; set; } = string.Empty;

    public string OriginalPrompt { get; set; } = string.Empty;

    public string NormalizedPrompt { get; set; } = string.Empty;

    public string ParsedAction { get; set; } = string.Empty;

    public string ParsedSubject { get; set; } = string.Empty;

    public string RawJsonResponse { get; set; } = string.Empty;

    public string HumanPhrase { get; set; } = string.Empty;

    public int HitCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime LastAccessedAt { get; set; }
}
