namespace Underdog.Application.Models;

public sealed class IntentMetricRow
{
    public ulong Id { get; set; }

    public string CorrelationId { get; set; } = string.Empty;

    public int DurationMs { get; set; }

    public bool FromCache { get; set; }

    public bool IsSuccess { get; set; }

    public string? ErrorCode { get; set; }

    public DateTime CreatedAt { get; set; }
}
