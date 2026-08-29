using Underdog.Application.Models;

namespace Underdog.Application.Abstractions;

public interface IIntentCacheRepository
{
    Task<AiIntentCacheRow?> GetByHashAsync(string normalizedPromptHash, CancellationToken cancellationToken = default);

    Task IncrementHitAsync(ulong id, CancellationToken cancellationToken = default);

    Task SaveAsync(string originalPrompt, string normalizedPrompt, string normalizedPromptHash, ParsedIntent parsedIntent, string rawJsonResponse, CancellationToken cancellationToken = default);
}
