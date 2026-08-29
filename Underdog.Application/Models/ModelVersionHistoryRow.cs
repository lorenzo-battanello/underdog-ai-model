namespace Underdog.Application.Models;

public sealed class ModelVersionHistoryRow
{
    public ulong Id { get; set; }

    public string ModelVersion { get; set; } = string.Empty;

    public string ModelPath { get; set; } = string.Empty;

    public string ModelFileHash { get; set; } = string.Empty;

    public DateTime FirstSeenAt { get; set; }

    public DateTime LastSeenAt { get; set; }
}
