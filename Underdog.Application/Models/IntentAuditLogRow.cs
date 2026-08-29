namespace Underdog.Application.Models;

public sealed class IntentAuditLogRow
{
    public ulong Id { get; set; }

    public string CorrelationId { get; set; } = string.Empty;

    public string OriginalPrompt { get; set; } = string.Empty;

    public string NormalizedPromptHash { get; set; } = string.Empty;

    public string RawJsonResponse { get; set; } = string.Empty;

    public string HumanPhrase { get; set; } = string.Empty;

    public bool FromCache { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; }
}
