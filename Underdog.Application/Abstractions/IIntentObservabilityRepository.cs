namespace Underdog.Application.Abstractions;

public interface IIntentObservabilityRepository
{
    Task SaveAuditAsync(string correlationId, string originalPrompt, string normalizedPromptHash, string rawJsonResponse, string humanPhrase, bool fromCache, string status, string? errorMessage, CancellationToken cancellationToken = default);

    Task SaveMetricAsync(string correlationId, int durationMs, bool fromCache, bool isSuccess, string? errorCode, CancellationToken cancellationToken = default);

    Task UpsertModelVersionAsync(string modelVersion, string modelPath, string modelFileHash, CancellationToken cancellationToken = default);
}
